using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize]
    [Authorize(Roles = "Customer,Lessor")]
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
                else
                {
                    ViewBag.UserId = user.Id;
                    ViewBag.userName = user.UserName;
                }
            }

            var productsInCart = _facadeService.Order.GetGroupedCartItems(selectedItems);
            var cartItems = productsInCart.SelectMany(g => g).ToList();
            if (!productsInCart.Any())
                return RedirectToAction("Cart", "Customer");

            ViewBag.CartItems = cartItems;
            return View(productsInCart);
        }

        [HttpPost]
        public IActionResult ProcessPayment(
            List<int> selectedItems,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod
        )
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }

            if (paymentMethod == PaymentMethod.PayByCash)
            {
                var paymentByCOD = _facadeService.Order.ProcessOrderByCOD(
                    selectedItems,
                    phoneNumber,
                    deliveryOptions,
                    deliveryAddress,
                    paymentMethod,
                    userId
                );

                if (paymentByCOD != null)
                {
                    TempData["success"] = "Thanh toán thành công";
                    return RedirectToAction("Cart", "Customer");
                }
                TempData["error"] = "Thanh toán thất bại";
                return RedirectToAction("Cart", "Customer");
            }
            else if (paymentMethod == PaymentMethod.VNPay)
            {
                var vnpayUrl = _facadeService.Order.ProcessOrderByVnPay(
                    selectedItems,
                    phoneNumber,
                    deliveryOptions,
                    deliveryAddress,
                    paymentMethod,
                    userId,
                    HttpContext
                );
                if (vnpayUrl != null)
                {
                    return Redirect(vnpayUrl);
                }
            }
            TempData["error"] = "Thanh toán thất bại";
            return RedirectToAction("Cart", "Customer");
        }

        public IActionResult VNPayReturn()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (
                userId != null
                && _facadeService.Order.VNPayReturn(Request.Query, userId, out int orderId)
            )
            {
                TempData["success"] = "Thanh toán thành công";
                return RedirectToAction("Cart", "Customer");
            }
            else
            {
                TempData["error"] = "Thanh toán thất bại";
                return RedirectToAction("Cart", "Customer");
            }
        }
    }
}
