using BussinessLayer.Interface;
using DataAccess.Models;
using MailKit.Search;
using Repository.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Implement
{
    public class OrderHistoryService : IOrderHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //view order history
        public IEnumerable<(Order Order, Dictionary<User, List<OrderDetail>> GroupedByStore)> GetOrdersGroupedByStore(Guid userId)
        {
            var orders = _unitOfWork.Order
                .GetRange(
                    o => o.UserId == userId,
                    includeProperties: "OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
                ).OrderByDescending(o => o.Status == OrderStatus.Cancelled && o.Status == OrderStatus.Completed)
                .ThenByDescending(o => o.OrderDate).ToList();

            var result = orders.Select(order =>
            {
                var grouped = order.OrderDetails?
                    .Where(od => od.Product?.User != null)
                    .GroupBy(od => od.Product!.User!)
                    .ToDictionary(g => g.Key, g => g.ToList())
                    ?? new Dictionary<User, List<OrderDetail>>();

                return (order, grouped);
            }).ToList();

            return result;
        }

        public bool CancelOrder(int orderId, Guid userId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return false;
            }
            order.Status = OrderStatus.Cancelled;
            order.OrderDate = DateTime.Now;
            _unitOfWork.Save();
            return true;
        }

        //order details
        public (Order Order, Dictionary<User, List<OrderDetail>> GroupedByStore)? GetOrderDetail(int orderId, Guid userId)
        {
            var order = _unitOfWork.Order.Get(
                o => o.Id == orderId && o.UserId == userId,
                includeProperties: "OrderDetails,OrderDetails.Product,OrderDetails.Product.User"
            );

            if (order == null)
            {
                return null;
            }

            var grouped = order.OrderDetails?
                .Where(od => od.Product?.User != null)
                .GroupBy(od => od.Product!.User!)
                .ToDictionary(g => g.Key, g => g.ToList())
                ?? new Dictionary<User, List<OrderDetail>>();

            return (order, grouped);
        }

    }
}
