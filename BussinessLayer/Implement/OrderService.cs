using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.Implement;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utility.Email;

namespace BussinessLayer.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vpnPayService;
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _configuration;

        public OrderService(IUnitOfWork unitOfWork, IVnPayService vpnPayService, IConfiguration configuration, IEmailQueue emailQueue)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _vpnPayService = vpnPayService;
            _emailQueue = emailQueue;
           
        }

        //view checkout
        public IEnumerable<IGrouping<User?, Cart>> GetGroupedCartItems(List<int> selectedItems)
        {
            var productsInCart = _unitOfWork.Cart
                .GetRange(
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
            List<int> selectedItems, string phoneNumber, 
            DeliveryOption deliveryOptions, string deliveryAddress,
            PaymentMethod paymentMethod, Guid userId)
        {
            var productsInCart = _unitOfWork.Cart.GetRange
                (c => selectedItems.Contains(c.Id), includeProperties: "Product,Product.User").ToList();
            if (!productsInCart.Any())
            {
                return null;
            }
                var order = CreateOrder(userId, phoneNumber, deliveryOptions, deliveryAddress, paymentMethod, productsInCart);
                _unitOfWork.Order.Add(order);
                var removeCartItem = _unitOfWork.Cart.GetRange(c => selectedItems.Contains(c.Id));
                _unitOfWork.Cart.RemoveRange(removeCartItem);

            try
            {
                _unitOfWork.Save();
                var user = _unitOfWork.User.Get(u => u.Id == userId);
                var customerName = user?.UserName ?? "Khách hàng";

                //Người bán
                SendEmailsToSellers(productsInCart, userId, paymentMethod, deliveryOptions, deliveryAddress, phoneNumber);

                //người mua
                var totalPrices = order.TotalPrice;
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    SendEmailToBuyer(productsInCart, userId, phoneNumber, paymentMethod, deliveryOptions, deliveryAddress, totalPrices);
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
            List<int> selectedItems, string phoneNumber, 
            DeliveryOption deliveryOptions, string deliveryAddress, 
            PaymentMethod paymentMethod, Guid userId, HttpContext httpContext)
        {
            var productsInCart = _unitOfWork.Cart.GetRange
                (c => selectedItems.Contains(c.Id), includeProperties: "Product,Product.User").ToList();

            if (!productsInCart.Any())
            {
                return null;
            }

            var order = CreateOrder(userId, phoneNumber, deliveryOptions, deliveryAddress, paymentMethod, productsInCart);
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
            return _vpnPayService.CreatePaymentUrl(httpContext, vnpayModel);
        }

        public bool VNPayReturn(IQueryCollection query, string userId, out int orderId)
        {
            orderId = 0;     
            var response = _vpnPayService.PaymentExecute(query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                if (int.TryParse(response?.OrderDescription, out int failOrderId))
                {
                    var orderDetailList = _unitOfWork.OrderDetail.GetRange(o => o.Id == failOrderId);
                    _unitOfWork.OrderDetail.RemoveRange(orderDetailList);
                    _unitOfWork.Save();

                    var order = _unitOfWork.Order.Get(o => o.Id == failOrderId);
                    if(order != null)
                    {
                        _unitOfWork.Order.Remove(order);
                        _unitOfWork.Save();
                    }
                }
                return false;
            }

            if (int.TryParse(response.OrderDescription, out int orderSuccessId))
            {
                var order =_unitOfWork.Order.Get(o => o.Id ==  orderSuccessId, includeProperties: "User");
                var orderDetail = _unitOfWork.OrderDetail
                    .GetRange(od => od.OrderId == orderSuccessId, includeProperties: "Product,Product.User");
                //người bán
                SendEmailsToSellers(
                    orderDetail.Select(od => new Cart
                    {
                        Product = od.Product,
                        Count = od.Quantity,
                        StartDate = od.StartDate,
                        EndDate = od.EndDate
                    }),
                    order!.UserId,
                    order.PaymentMethod,
                    order.DeliveryOption,
                    order.ShippingAddress!,
                    order.PhoneNumber!
                );
                //người mua
                SendEmailToBuyer(
                    orderDetail.Select(od => new Cart
                    {
                        Product = od.Product,
                        Count = od.Quantity,
                        StartDate = od.StartDate,
                        EndDate = od.EndDate
                    }),
                    order.UserId,
                    order.PhoneNumber!,
                    order.PaymentMethod,
                    order.DeliveryOption,
                    order.ShippingAddress!,
                    order.TotalPrice
                );
                var purchasedProductIds = _unitOfWork.OrderDetail
                    .GetRange(od =>  od.OrderId == orderSuccessId, includeProperties: "Product")
                    .Select(od => od.ProductId)
                    .ToList();
  
                var cartItemRemove = _unitOfWork.Cart
                    .GetRange(c => c.UserId.ToString() == userId && purchasedProductIds.Contains(c.ProductId));

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
            IEnumerable<Cart> productsInCart)
        {
            double totalPrice = productsInCart.Sum(p =>
            {
                var price = p.Product?.Price ?? 0;
                var days = (p.EndDate - p.StartDate)?.Days ?? 1;
                return price * p.Count * days;
            });

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
                OrderDetails = productsInCart.Select(p => new OrderDetail
                {
                    ProductId = p.ProductId,
                    Quantity = p.Count,
                    UnitPrice = p.Product != null ? p.Product.Price : 0,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    SizeClother = p.SizeClother,
                    SizeShoe = p.SizeShoe
                    
                }).ToList()
            };
        }
        private void SendEmailsToSellers(
            IEnumerable<Cart> productsInCart,
            Guid userId,
            PaymentMethod paymentMethod,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            string phoneNumber)
        {
            var storeProductMap = productsInCart
                .Where(item => item.Product != null)
                .GroupBy(item => item.Product?.User?.Email ?? "abc@gmail.com")
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            var user = _unitOfWork.User.Get(u => u.Id == userId);
            var customerName = user?.UserName ?? "Khách hàng";

            if (storeProductMap != null)
            {
                foreach (var entry in storeProductMap)
                {
                    string storeEmail = entry.Key;
                    var items = entry.Value;
                    Console.WriteLine($"Email gửi đến: {storeEmail}");
                    Console.WriteLine("Sản phẩm:");
                    foreach (var item in items)
                    {
                        Console.WriteLine($"- {item.Product?.Name}");
                    }
                    double storeTotal = items.Sum(i =>
                    {
                        var rentalDays = (i.EndDate - i.StartDate)?.Days ?? 1;
                        return i.Product!.Price * i.Count * rentalDays;
                    });

                    string productDetails = string.Join("", items.Select(i =>
                    {
                        var product = i.Product!;
                        var startDate = i.StartDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var endDate = i.EndDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var rentalDays = (i.EndDate - i.StartDate)?.Days ?? 1;
                        double itemTotal = product.Price * i.Count * rentalDays;

                        return $@"
                    <tr>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{product.Name}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{product.Price:N0} đ</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{i.Count}</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{rentalDays} ngày ({startDate} - {endDate})</td>
                        <td style='padding: 8px; border: 1px solid #ddd;'>{itemTotal:N0} đ</td>
                    </tr>";
                    }));

                    string body = SellerEmailInformation(userId, storeTotal, productDetails, paymentMethod, deliveryOptions, deliveryAddress, phoneNumber);
                    string subject = $"Đơn hàng mới từ khách hàng {customerName}";

                    _emailQueue.QueueEmail(storeEmail, subject, body);
                }
            }
            
        }

        private void SendEmailToBuyer(
            IEnumerable<Cart> productsInCart,
            Guid userId,
            string phoneNumber,
            PaymentMethod paymentMethod,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            double totalPrices)
        {
            var user = _unitOfWork.User.Get(u => u.Id == userId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return;

            var productGroupsByStore = productsInCart
                .Where(i => i.Product != null)
                .GroupBy(i => i.Product!.User!.Id)
                .ToList();
            int storeCount = productGroupsByStore.Count;
            if (productGroupsByStore != null)
            {
                string groupedProductDetail = string.Join("<br/>", productGroupsByStore.Select(group =>
                {
                    Guid? storeId = group.Key;
                    var storeName = _unitOfWork.User.Get(u => u.Id == storeId)?.UserName ?? "Không rõ";

                    string productRows = string.Join("", group.Select(i =>
                    {
                        var product = i.Product!;
                        var startDate = i.StartDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var endDate = i.EndDate?.ToString("dd/MM/yyyy") ?? "N/A";
                        var rentalDays = (i.EndDate - i.StartDate)?.Days ?? 1;
                        double itemTotal = product.Price * i.Count * rentalDays;

                        return $@"
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{product.Name}</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{product.Price:N0} đ</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{i.Count}</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{rentalDays} ngày ({startDate} - {endDate})</td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{itemTotal:N0} đ</td>
                        </tr>";
                        }));

                    return $@"
                    <h3 style='color: #2e7d32;'>Cửa hàng: {storeName}</h3>
                    <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                        <thead>
                            <tr style='background-color: #e8f5e9;'>
                                <th style='padding: 8px; border: 1px solid #ddd;'>Tên sản phẩm</th>
                                <th style='padding: 8px; border: 1px solid #ddd;'>Giá</th>
                                <th style='padding: 8px; border: 1px solid #ddd;'>Số lượng</th>
                                <th style='padding: 8px; border: 1px solid #ddd;'>Thời gian thuê</th>
                                <th style='padding: 8px; border: 1px solid #ddd;'>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {productRows}
                        </tbody>
                    </table>";
                }));

                string buyerBody = BuyerEmailInformation(
                userId,
                phoneNumber,
                paymentMethod,
                deliveryOptions,
                deliveryAddress,
                groupedProductDetail,
                totalPrices,
                storeCount
                );

                string buyerSubject = "Xác nhận đơn hàng của bạn";
                _emailQueue.QueueEmail(user.Email, buyerSubject, buyerBody);
            }       
        }
        private string SellerEmailInformation(Guid userId, double storeTotal, string productDetails, PaymentMethod paymentMethod, DeliveryOption deliveryOptions, string deliveryAddress,string phoneNumber)
        {
            var user = _unitOfWork.User.Get(u => u.Id == userId);
            var customerName = user?.UserName ?? "Khách hàng";
            return $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #2e7d32;'>Đơn hàng mới từ khách hàng {customerName}</h2>
                
                <p style='margin-bottom: 8px;'><strong>Tên khách hàng: </strong> {customerName}</p>
                <p style='margin-bottom: 8px;'><strong>Số điện thoại: </strong> {phoneNumber}</p>
                <p style='margin-bottom: 8px;'><strong>Phương thức thanh toán: </strong> {(paymentMethod == PaymentMethod.PayByCash ? "Thanh toán tiền mặt" : "VNPay")}</p>
                <p style='margin-bottom: 8px;'><strong>Hình thức giao hàng: </strong> {(deliveryOptions == DeliveryOption.StorePickup ? "Nhận tại cửa hàng" : "Ship tận nơi")}</p>
                <p style='margin-bottom: 16px;'><strong>Địa chỉ giao hàng: </strong> {(deliveryOptions == DeliveryOption.HomeDelivery ? deliveryAddress : "Nhận tại cửa hàng")}</p>

                <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                <thead>
                    <tr style='background-color: #e8f5e9;'>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Tên sản phẩm</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Giá</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Số lượng</th>
                        <th style='padding: 8px; border: 1px solid #ddd;'>Thời gian thuê</th>
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

        private string BuyerEmailInformation(
        Guid userId,
        string phoneNumber,
        PaymentMethod paymentMethod,
        DeliveryOption deliveryOptions,
        string deliveryAddress,
        string groupedProductDetail,
        double totalPrices,
        int storeCount
        )
        {
            var user = _unitOfWork.User.Get(u => u.Id == userId);
            var customerName = user?.UserName ?? "Khách hàng";

            string shippingFeeDis;
            var shippingFee = storeCount >= 2 ? 50.000 : 25.000;
            shippingFeeDis = $"{shippingFee.ToString("N0", new CultureInfo("vi-VN"))}đ";
            
            string shippingHtml = "";
            if(deliveryOptions == DeliveryOption.HomeDelivery)
            {
                shippingHtml = $"<p style = 'margin-bottom: 16px;' ><strong> Phí vận chuyển: </strong> {shippingFeeDis}</ p>";
            }
            return $@"
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2 style='color: #2e7d32;'>Cảm ơn {customerName} đã đặt hàng tại Green Closet!</h2>

                <p style='margin-bottom: 8px;'><strong>Tên khách hàng: </strong> {customerName}</p>
                <p style='margin-bottom: 8px;'><strong>Số điện thoại: </strong> {phoneNumber}</p>
                <p style='margin-bottom: 8px;'><strong>Phương thức thanh toán: </strong> {(paymentMethod == PaymentMethod.PayByCash ? "Thanh toán tiền mặt" : "VNPay")}</p>
                <p style='margin-bottom: 8px;'><strong>Hình thức giao hàng: </strong> {(deliveryOptions == DeliveryOption.StorePickup ? "Nhận tại cửa hàng" : "Vận chuyển tận nơi")}</p>
                <p style='margin-bottom: 16px;'><strong>Địa chỉ giao hàng: </strong> {(deliveryOptions == DeliveryOption.HomeDelivery ? deliveryAddress : "Nhận tại cửa hàng")}</p>
                
                {shippingHtml}
                
                {groupedProductDetail}
                <p style='margin-bottom: 8px; font-size: 25px;'>Tổng giá trị đơn hàng: <strong style='color: #c62828;'>{totalPrices:N0} đ</strong></p>

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

        
    }
}
