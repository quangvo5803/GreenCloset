using BussinessLayer.Implement;
using BussinessLayer.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenCloset.Controllers
{
    public partial class CustomerController : BaseController
    {
        public CustomerController(IFacedeService facedeService)
        : base(facedeService) { }
        public IActionResult Cart()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }
            var cartResult = _facadeService.Cart.GetAllCartById(userId);
            return View(cartResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCart(Guid userId, int productId)
        {
            try
            {
                _facadeService.Cart.DeleteCart(userId, productId);
                return Json(new { success = true, message = "Đã xóa" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, string? size, DateTime startDate, DateTime endDate, int count = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _facadeService.Cart.AddToCart(productId, size, startDate, endDate, count, userId);
                return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int productId, int quantity, string? size, string? sizeType, string? startDate, string? endDate)
        {
            try
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Bạn cần đăng nhập trước" });
                }
                DateTime? start = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : null;
                DateTime? end = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : null;
                _facadeService.Cart.UpdateCart(productId, quantity, size, sizeType, start, end, userId);
                return Json(new { success = true, message = "Cập nhật thành công" });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật giỏ hàng.", error = ex.Message });
            }
        }



    }
}
