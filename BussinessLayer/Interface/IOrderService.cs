using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IOrderService
    {
        IEnumerable<IGrouping<User?, Cart>> GetGroupedCartItems(List<int> selectedItems);
        Order? ProcessOrderByCOD(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            Guid userId
        );

        string? ProcessOrderByVnPay(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            Guid userId,
            HttpContext httpContext
        );
        bool VNPayReturn(IQueryCollection query, string userId, out int orderId);
        IEnumerable<Order> GetOrdersByProductOwner(string? email = null);

        public IEnumerable<(
            Order Order,
            Dictionary<User, List<OrderDetail>> GroupedByStore
        )> GetOrdersGroupedByStore(Guid userId);
        bool MarkAsDelivered(int orderId);
        bool MarkAsRenting(int orderId);
        bool MarkAsReturning(int orderId);

        bool CancelOrder(int orderId, string reason);
        bool CompleteOrder(int orderId);
        (
            Order Order,
            Dictionary<User, List<OrderDetail>> GroupedByStore,
            List<int> checkReview
        )? GetOrderDetail(int orderId, Guid userId);
        Order? GetOrder(int orderId);
        bool VNPayReturnMonthlyFee(IQueryCollection query);
    }
}
