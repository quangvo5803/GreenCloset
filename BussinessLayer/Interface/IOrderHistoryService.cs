using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface IOrderHistoryService
    {
        public IEnumerable<(Order Order, Dictionary<User, List<OrderDetail>> GroupedByStore)> GetOrdersGroupedByStore(Guid userId);
        public bool CancelOrder(int orderId, Guid userId, string reason);
        public bool CompleteOrder(int orderId, Guid userId);
        public (Order Order, Dictionary<User, List<OrderDetail>> GroupedByStore, List<int> checkReview)? GetOrderDetail(int orderId, Guid userId);
    }
}
