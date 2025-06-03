using System.Globalization;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Implement;
using Utility.Email;

namespace BussinessLayer.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vpnPayService;
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;

        public OrderService(
            IUnitOfWork unitOfWork,
            IVnPayService vpnPayService,
            IConfiguration configuration,
            IEmailQueue emailQueue
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _vpnPayService = vpnPayService;
            _emailQueue = emailQueue;
        }

        //view checkout
        public IEnumerable<IGrouping<User?, Cart>> GetGroupedCartItems(List<int> selectedItems)
        {
            var productsInCart = _unitOfWork
                .Cart.GetRange(
                    c => selectedItems.Contains(c.Id),
                    includeProperties: "Product,Product.ProductAvatar,Product.User"
                )
                .ToList();

            var groupedByStore = productsInCart
                .Where(item => item.Product != null && item.Product.User != null)
                .GroupBy(item => item.Product?.User);

            return groupedByStore;
        }

        //Payment by COD
        public Order? ProcessOrderByCOD(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            Guid userId
        )
        {
            var productsInCart = _unitOfWork
                .Cart.GetRange(
                    c => selectedItems.Contains(c.Id),
                    includeProperties: "Product,Product.User"
                )
                .ToList();
            if (!productsInCart.Any())
            {
                return null;
            }
            var order = CreateOrder(
                userId,
                phoneNumber,
                deliveryOptions,
                deliveryAddress,
                paymentMethod,
                productsInCart
            );
            _unitOfWork.Order.Add(order);
            var removeCartItem = _unitOfWork.Cart.GetRange(c => selectedItems.Contains(c.Id));
            _unitOfWork.Cart.RemoveRange(removeCartItem);

            try
            {
                _unitOfWork.Save();
                var user = _unitOfWork.User.Get(u => u.Id == userId);
                var customerName = user?.UserName ?? "Khách hàng";

                //Người bán
                SendEmailsToSellers(order);

                //người mua
                var totalPrices = order.TotalPrice;
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    SendEmailToBuyer(order);
                }
                return order;
            }
            catch (Exception ex)
            {
                throw new Exception("Thanh toán COD thất bại", ex);
            }
        }

        //Payment by VnPay
        public string? ProcessOrderByVnPay(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            Guid userId,
            HttpContext httpContext
        )
        {
            var productsInCart = _unitOfWork
                .Cart.GetRange(
                    c => selectedItems.Contains(c.Id),
                    includeProperties: "Product,Product.User"
                )
                .ToList();

            if (!productsInCart.Any())
            {
                return null;
            }

            var order = CreateOrder(
                userId,
                phoneNumber,
                deliveryOptions,
                deliveryAddress,
                paymentMethod,
                productsInCart
            );
            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();
            var totalPrice = order.TotalPrice;
            var vnpayModel = new VnPaymentRequestModel
            {
                OrderId = new Random().Next(10000, 100000),
                Description = "Thanh toán VNPay",
                CreateDate = DateTime.Now,
                Amount = totalPrice,
                Order = order,
            };
            return _vpnPayService.CreatePaymentUrl(httpContext, vnpayModel, "Buy");
        }

        public bool VNPayReturn(IQueryCollection query, string userId, out int orderId)
        {
            orderId = 0;
            var response = _vpnPayService.PaymentExecute(query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                if (int.TryParse(response?.OrderDescription, out int failOrderId))
                {
                    var orderDetailList = _unitOfWork.OrderDetail.GetRange(o =>
                        o.Id == failOrderId
                    );
                    _unitOfWork.OrderDetail.RemoveRange(orderDetailList);
                    _unitOfWork.Save();

                    var order = _unitOfWork.Order.Get(o => o.Id == failOrderId);
                    if (order != null)
                    {
                        _unitOfWork.Order.Remove(order);
                        _unitOfWork.Save();
                    }
                }
                return false;
            }

            if (int.TryParse(response.OrderDescription, out int orderSuccessId))
            {
                var order = _unitOfWork.Order.Get(
                    o => o.Id == orderSuccessId,
                    includeProperties: "User"
                );
                var orderDetail = _unitOfWork.OrderDetail.GetRange(
                    od => od.OrderId == orderSuccessId,
                    includeProperties: "Product,Product.User"
                );
                //người bán
                SendEmailsToSellers(order!);
                //người mua
                SendEmailToBuyer(order!);
                var purchasedProductIds = _unitOfWork
                    .OrderDetail.GetRange(
                        od => od.OrderId == orderSuccessId,
                        includeProperties: "Product"
                    )
                    .Select(od => od.ProductId)
                    .ToList();

                var cartItemRemove = _unitOfWork.Cart.GetRange(c =>
                    c.UserId.ToString() == userId && purchasedProductIds.Contains(c.ProductId)
                );

                _unitOfWork.Cart.RemoveRange(cartItemRemove);
                _unitOfWork.Save();

                orderId = orderSuccessId;
                return true;
            }

            return false;
        }

        //Tạo order
        private Order CreateOrder(
            Guid userId,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            IEnumerable<Cart> productsInCart
        )
        {
            double prePrice = 0;
            double depositTotal = 0;

            foreach (var p in productsInCart)
            {
                var price = p.Product?.Price ?? 0;
                var deposit = p.Product?.DepositPrice ?? 0;

                int days = 1;
                if (p.StartDate.HasValue && p.EndDate.HasValue)
                {
                    days = (p.EndDate.Value - p.StartDate.Value).Days;
                    if (days == 0)
                        days = 1;
                }

                prePrice += price * p.Count * days;
                depositTotal += deposit * p.Count;
            }
            double totalPrice = prePrice + depositTotal;

            if (deliveryOptions == DeliveryOption.HomeDelivery)
            {
                int storeCount = productsInCart
                    .Select(p => p.Product?.UserId)
                    .Where(id => id != null)
                    .Distinct()
                    .Count();
                if (storeCount < 2)
                {
                    totalPrice += 25000;
                }
                else
                {
                    totalPrice += 50000;
                }
            }
            return new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalPrice = totalPrice,
                PhoneNumber = phoneNumber,
                DeliveryOption = deliveryOptions,
                ShippingAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                TotalDeposit = depositTotal,
                PrePrice = prePrice,
                OrderDetails = productsInCart
                    .Select(p => new OrderDetail
                    {
                        ProductId = p.ProductId,
                        Quantity = p.Count,
                        UnitPrice = p.Product != null ? p.Product.Price : 0,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        SizeClother = p.SizeClother,
                        SizeShoe = p.SizeShoe,
                    })
                    .ToList(),
            };
        }

        private void SendEmailsToSellers(Order order)
        {
            var user = _unitOfWork.User.Get(u => u.Id == order.UserId);
            var customerName = user?.UserName ?? "Khách hàng";

            var storeProductMap = order
                ?.OrderDetails?.Where(od => od.Product != null && od.Product.User != null)
                .GroupBy(od => od.Product?.User?.Email ?? "abc@gmail.com")
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var entry in storeProductMap!)
            {
                string storeEmail = entry.Key;
                var items = entry.Value;

                double storeTotal = items.Sum(i =>
                {
                    var days = (i.EndDate - i.StartDate)?.Days ?? 1;
                    days = days > 0 ? days : 1;
                    return i.UnitPrice * i.Quantity * days
                        + (i.Product?.DepositPrice * i.Quantity ?? 0);
                });

                string productDetails = string.Join(
                    "",
                    items.Select(i =>
                    {
                        var product = i.Product!;
                        var startDate = i.StartDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var endDate = i.EndDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var days = (i.EndDate - i.StartDate)?.Days ?? 1;
                        days = days > 0 ? days : 1;
                        double itemTotal = i.UnitPrice * i.Quantity * days;
                        double deposit = product.DepositPrice * i.Quantity;
                        double prePrice = itemTotal + deposit;
                        var sizeInfo =
                            i.SizeClother != null ? $"{i.SizeClother.Value}"
                            : i.SizeShoe != null ? $"{i.SizeShoe.Value}"
                            : "Không có";

                        return $@"
                        <tr>
                            <td text-center style='padding: 8px; border: 1px solid #ddd;'>{product.Name}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{i.UnitPrice:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{product.Color} - {sizeInfo}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{i.Quantity}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{days} ngày ({startDate} - {endDate})</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{itemTotal:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{deposit:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{prePrice:N0} đ</td>
                        </tr>";
                    })
                );

                string body = SellerEmailInformation(order!, productDetails, storeTotal);
                string subject = $"Đơn hàng mới từ khách hàng {customerName}";

                _emailQueue.QueueEmail(storeEmail, subject, body);
            }
        }

        private string SellerEmailInformation(Order order, string productDetails, double storeTotal)
        {
            var user = _unitOfWork.User.Get(u => u.Id == order.UserId);
            var customerName = user?.UserName ?? "Khách hàng";
            return $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #2e7d32;'>Đơn hàng mới từ khách hàng {customerName}</h2>
                
                <p style='margin-bottom: 8px;'><strong>Tên khách hàng: </strong> {customerName}</p>
                <p style='margin-bottom: 8px;'><strong>Số điện thoại: </strong> {order.PhoneNumber}</p>
                <p style='margin-bottom: 8px;'><strong>Phương thức thanh toán: </strong> {(order.PaymentMethod == PaymentMethod.PayByCash ? "Thanh toán tiền mặt" : "VNPay")}</p>
                <p style='margin-bottom: 8px;'><strong>Hình thức giao hàng: </strong> {(order.DeliveryOption == DeliveryOption.StorePickup ? "Nhận tại cửa hàng" : "Ship tận nơi")}</p>
                <p style='margin-bottom: 16px;'><strong>Địa chỉ giao hàng: </strong> {(order.DeliveryOption == DeliveryOption.HomeDelivery ? order.ShippingAddress : "Nhận tại cửa hàng")}</p>

                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                <thead>
                    <tr style='background-color: #e8f5e9;'>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Tên sản phẩm</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Giá</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Phân loại</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Số lượng</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Thời gian thuê</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Tổng tiền hàng</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Tổng tiền cọc</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {productDetails}
                </tbody>
                </table>
                <p style='margin-bottom: 8px; font-size: 25px;'>Tổng giá trị: <strong style='color: #c62828;'>{storeTotal:N0} đ</strong></p>
                
                    <p style='font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;'>
                        Cảm ơn bạn đã tin tưởng Green Closet
                    </p>
                    <p style='font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;'>
                        Green Closet
                    </p>
                    <div style='text-align: center; margin-top: 20px;'>
                        <img src='https://i.imgur.com/0Iphozz.png' alt='Green Closet Logo' style='max-width: 200px; height: auto; border-radius: 8px;'>
                    </div>
            </div>";
        }

        private void SendEmailToBuyer(Order order)
        {
            var user = _unitOfWork.User.Get(u => u.Id == order.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return;

            var productGroupsByStore = order
                ?.OrderDetails?.GroupBy(od => od.Product?.UserId)
                .ToList();

            int storeCount = productGroupsByStore!.Count;

            string groupedProductDetail = string.Join(
                "<br/>",
                productGroupsByStore.Select(group =>
                {
                    var storeId = group.Key;
                    var storeName =
                        _unitOfWork.User.Get(u => u.Id == storeId)?.ShopName
                        ?? _unitOfWork.User.Get(u => u.Id == storeId)?.UserName;

                    string productRows = string.Join(
                        "",
                        group.Select(i =>
                        {
                            var product = i.Product;

                            var startDate = i.StartDate?.ToString("dd/MM/yyyy") ?? "N/A";
                            var endDate = i.EndDate?.ToString("dd/MM/yyyy") ?? "N/A";
                            var rentalDays = (i.EndDate - i.StartDate)?.Days ?? 1;
                            int days = rentalDays > 0 ? rentalDays : 1;
                            double itemTotal = i.UnitPrice * i.Quantity * days;
                            var sizeInfo =
                                i.SizeClother != null ? $"{i.SizeClother.Value}"
                                : i.SizeShoe != null ? $"{i.SizeShoe.Value}"
                                : "Không có";

                            return $@"
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{product?.Name}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{i.UnitPrice:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{i.Quantity}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{product!.Color} - {sizeInfo}</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{days} ngày ({startDate} - {endDate})</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{itemTotal:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{product?.DepositPrice * i.Quantity:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{itemTotal + product?.DepositPrice * i.Quantity:N0} đ</td>
                        </tr>";
                        })
                    );

                    return $@"
                <h3 style='color: #2e7d32;'>Cửa hàng: {storeName}</h3>
                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                    <thead>
                        <tr style='background-color: #e8f5e9;'>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Tên sản phẩm</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Giá</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Số lượng</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Phân loại</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Thời gian thuê</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Tổng tiền hàng</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Tổng tiền cọc</th>
                            <th style='padding: 8px; border: 1px solid #ddd;'>Thành tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        {productRows}
                    </tbody>
                </table>";
                })
            );

            string buyerBody = BuyerEmailInformation(order!, groupedProductDetail, storeCount);

            string buyerSubject = "Xác nhận đơn hàng của bạn";
            _emailQueue.QueueEmail(user.Email, buyerSubject, buyerBody);
        }

        private string BuyerEmailInformation(
            Order order,
            string groupedProductDetail,
            int storeCount
        )
        {
            var user = _unitOfWork.User.Get(u => u.Id == order.UserId);
            var customerName = user?.UserName ?? "Khách hàng";

            string shippingFeeDis;
            var shippingFee = storeCount >= 2 ? 50000 : 25000;
            shippingFeeDis = $"{shippingFee.ToString("N0", new CultureInfo("vi-VN"))}đ";

            string shippingHtml = "";
            if (order.DeliveryOption == DeliveryOption.HomeDelivery)
            {
                shippingHtml =
                    $"<p style = 'margin-bottom: 16px;' ><strong> Phí vận chuyển: </strong> {shippingFeeDis:NO}</ p>";
            }
            return $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #2e7d32;'>Cảm ơn {customerName} đã đặt hàng tại Green Closet!</h2>

                <p style='margin-bottom: 8px;'><strong>Tên khách hàng: </strong> {customerName}</p>
                <p style='margin-bottom: 8px;'><strong>Số điện thoại: </strong> {order.PhoneNumber}</p>
                <p style='margin-bottom: 8px;'><strong>Phương thức thanh toán: </strong> {(order.PaymentMethod == PaymentMethod.PayByCash ? "Thanh toán tiền mặt" : "VNPay")}</p>
                <p style='margin-bottom: 8px;'><strong>Hình thức giao hàng: </strong> {(order.DeliveryOption == DeliveryOption.StorePickup ? "Nhận tại cửa hàng" : "Vận chuyển tận nơi")}</p>
                <p style='margin-bottom: 16px;'><strong>Địa chỉ giao hàng: </strong> {(order.DeliveryOption == DeliveryOption.HomeDelivery ? order.ShippingAddress : "Nhận tại cửa hàng")}</p>
                
                {shippingHtml}
                
                {groupedProductDetail}
                <p style='margin-bottom: 8px; font-size: 25px;'>Tổng giá trị đơn hàng: <strong style='color: #c62828;'>{order.TotalPrice:N0} đ</strong></p>

                <p style='font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;'>
                    Cảm ơn bạn đã tin tưởng Green Closet.
                </p>
                <p style='font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;'>
                    Green Closet
                </p>
                <div style='text-align: center; margin-top: 20px;'>
                    <img src='https://i.imgur.com/0Iphozz.png' alt='Green Closet Logo' style='max-width: 200px; height: auto; border-radius: 8px;'>
                </div>
            </div>";
        }

        public IEnumerable<Order> GetOrdersByProductOwner(string? email = null)
        {
            if (string.IsNullOrEmpty(email))
                return _unitOfWork.Order.GetAll(includeProperties: "OrderDetails.Product");

            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
                return new List<Order>();

            var orders = _unitOfWork
                .Order.GetRange(
                    order =>
                        order.OrderDetails != null
                        && order.OrderDetails.Any(od =>
                            od.Product != null && od.Product.UserId == user.Id
                        ),
                    includeProperties: "OrderDetails.Product"
                )
                .ToList();

            return orders;
        }

        //view order history
        public IEnumerable<(
            Order Order,
            Dictionary<User, List<OrderDetail>> GroupedByStore
        )> GetOrdersGroupedByStore(Guid userId)
        {
            var orders = _unitOfWork
                .Order.GetRange(
                    o => o.UserId == userId,
                    includeProperties: "OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
                )
                .OrderByDescending(o =>
                    o.Status == OrderStatus.Completed ? o.CompleteDate
                    : o.Status == OrderStatus.Cancelled ? o.CancelDate
                    : o.OrderDate
                )
                .ToList();

            var result = orders
                .Select(order =>
                {
                    var grouped =
                        order
                            .OrderDetails?.Where(od => od.Product?.User != null)
                            .GroupBy(od => od.Product!.User!)
                            .ToDictionary(g => g.Key, g => g.ToList())
                        ?? new Dictionary<User, List<OrderDetail>>();

                    return (order, grouped);
                })
                .ToList();

            return result;
        }

        public bool MarkAsDelivered(int orderId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }
            order.Status = OrderStatus.Delivering;
            order.DeliveryDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        public bool MarkAsRenting(int orderId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }
            order.Status = OrderStatus.Renting;
            order.RentingDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        public bool MarkAsReturning(int orderId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId);
            if (order == null)
            {
                return false;
            }
            order.Status = OrderStatus.Returning;
            order.ReturingDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        public bool CancelOrder(int orderId, string reason)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }
            order.Status = OrderStatus.Cancelled;
            order.CancelReason = reason;
            order.CancelDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        public bool CompleteOrder(int orderId)
        {
            var completeOrder = _unitOfWork.Order.Get(o => o.Id == orderId);
            if (completeOrder == null)
            {
                return false;
            }
            completeOrder.Status = OrderStatus.Completed;
            completeOrder.CompleteDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        //order details
        public (
            Order Order,
            Dictionary<User, List<OrderDetail>> GroupedByStore,
            List<int> checkReview
        )? GetOrderDetail(int orderId, Guid userId)
        {
            var order = _unitOfWork.Order.Get(
                o => o.Id == orderId,
                includeProperties: "OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
            );

            if (order == null)
            {
                return null;
            }

            var grouped =
                order
                    .OrderDetails?.Where(od => od.Product?.User != null)
                    .GroupBy(od => od.Product!.User!)
                    .ToDictionary(g => g.Key, g => g.ToList())
                ?? new Dictionary<User, List<OrderDetail>>();

            //feeback check
            var checkProduct = order.OrderDetails?.Select(od => od.ProductId).ToList();

            var checkReview = _unitOfWork
                .Feedback.GetRange(f => f.UserId == userId && f.OrderId == orderId)
                .Select(f => f.ProductId)
                .ToList();

            return (order, grouped, checkReview);
        }

        public Order? GetOrder(int orderId)
        {
            var order = _unitOfWork.Order.Get(
                o => o.Id == orderId,
                includeProperties: "User,OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
            );
            return order;
        }

        public IEnumerable<Order> GetAllOrAdmin()
        {
            var rs = _unitOfWork
                .Order.GetAll(
                    includeProperties: "OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
                )
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return rs;
        }

        public (
            Order Order,
            Dictionary<User, List<OrderDetail>> GroupedByStore
        )? GetOrderDetailsAdmin(int orderId)
        {
            var order = _unitOfWork.Order.Get(
                o => o.Id == orderId,
                includeProperties: "User,OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
            );

            if (order == null)
            {
                return null;
            }

            var grouped =
                order
                    .OrderDetails?.Where(od => od.Product?.User != null)
                    .GroupBy(od => od.Product!.User!)
                    .ToDictionary(g => g.Key, g => g.ToList())
                ?? new Dictionary<User, List<OrderDetail>>();

            return (order, grouped);
        }
    }
}
