using DataAccess.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        string GetNameByUserId(string userId);
        IEnumerable<Cart> GetCartItems(string selectedItems);

        Order ProcessOrderByCOD(string selectedItems, string phoneNumber, 
            string deliveryOptions, string deliveryAddress, string paymentMethod, Guid userId);

        string ProcessOrderByVnPay(string selectedItems, string phoneNumber, string deliveryOptions, 
            string deliveryAddress, string paymentMethod, Guid userId, HttpContext httpContext);

        bool VNPayReturn(IQueryCollection query, string userId, out int orderId, HttpContext httpContext);
    }
}
