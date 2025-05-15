using DataAccess.Models;
using Repository.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace BussinessLayer.Interface
{
    public interface IOrderService
    {
        IEnumerable<IGrouping<User?, Cart>> GetGroupedCartItems(List<int> selectedItems);
        Order? ProcessOrderByCOD(List<int> selectedItems, string phoneNumber, 
            DeliveryOption deliveryOptions, string deliveryAddress, PaymentMethod paymentMethod, Guid userId);

        string? ProcessOrderByVnPay(List<int> selectedItems, string phoneNumber, DeliveryOption deliveryOptions, 
            string deliveryAddress, PaymentMethod paymentMethod, Guid userId, HttpContext httpContext);
        bool VNPayReturn(IQueryCollection query, string userId, out int orderId);
    }
}
