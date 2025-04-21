using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Customer")]
    public partial class CustomerController : BaseController
    {
        [HttpPost]
        public IActionResult Checkout(string selectedItems)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var userName = _facadeService.Order.GetNameByUserId(userId);
            if (userName == null)
            {
                TempData["error"] = "userName null";
            }

            var productsInCart = _facadeService.Order.GetCartItems(selectedItems);
            if (!productsInCart.Any())
            {
                return RedirectToAction("Cart", "Customer");
            }
            ViewBag.UserId = userId;
            ViewBag.user = userName;
            return View(productsInCart);
        }

        [HttpPost]
        public IActionResult ProcessPayment(
            string selectedItems,
            string phoneNumber,
            string deliveryOptions,
            string deliveryAddress,
            string paymentMethod)
        {            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }

            if(paymentMethod == "cod")
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
            else if(paymentMethod == "vnpay")
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
            if (_facadeService.Order.VNPayReturn(Request.Query, userId, out int orderId, HttpContext))
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
