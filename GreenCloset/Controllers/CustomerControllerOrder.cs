using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Customer")]
    public partial class CustomerController : BaseController
    {
        [HttpPost]
        public IActionResult Checkout(List<int> selectedItems)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = _facadeService.User.GetUserByEmail(email);
                if (user == null)
                {
                    TempData["error"] = "Không tìm thấy người dùng";
                }
                else {
                    ViewBag.UserId = user.Id;
                    ViewBag.userName = user.UserName;
                }           
            }
      
            var productsInCart = _facadeService.Order.GetCartItems(selectedItems);
            if (!productsInCart.Any()) return RedirectToAction("Cart", "Customer");

            ViewBag.CartItems = productsInCart;
            return View(productsInCart);
        }

        [HttpPost]
        public IActionResult ProcessPayment(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod)
        {            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }

            if(paymentMethod == PaymentMethod.PayByCash)
            {
                var paymentByCOD = _facadeService.Order.ProcessOrderByCOD(
                    selectedItems, phoneNumber, deliveryOptions, 
                    deliveryAddress, paymentMethod, userId);

                if (paymentByCOD != null) 
                {
                    TempData["success"] = "Thanh toan ok";
                    return RedirectToAction("Cart", "Customer");
                }
                TempData["error"] = "Thanh toan by COD fail";
                return RedirectToAction("Checkout", "Customer");
            }
            else if(paymentMethod == PaymentMethod.VNPay)
            {
                var vnpayUrl = _facadeService.Order.ProcessOrderByVnPay(
                    selectedItems, phoneNumber, deliveryOptions, 
                    deliveryAddress, paymentMethod, userId, HttpContext);
                if (vnpayUrl != null)
                {
                    return Redirect(vnpayUrl);
                }

            }
            TempData["error"] = "thanh toan fail";
            return RedirectToAction("Checkout", "Customer");
        }

        public IActionResult VNPayReturn()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null && _facadeService.Order.VNPayReturn(Request.Query, userId, out int orderId))
            {
                TempData["success"] = "Payment successful.";
                return RedirectToAction("Cart", "Customer");
            }
            else
            {
                TempData["error"] = "Payment failed.";
                return RedirectToAction("Checkout");
            }
        }

    }
}
