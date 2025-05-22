using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Customer")]
    public partial class CustomerController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int orderId, int productId, int stars, string? feedbackContent, List<IFormFile>? images)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid && userId != null)
            {
                await _facadeService.FeedBack.SubmitFeedback(orderId ,productId, stars, feedbackContent, images, Guid.Parse(userId));
                TempData["success"] = "Đánh giá thành công";
                return RedirectToAction("OrderDetails", "Customer", new {orderId = orderId});
            }

            TempData["error"] = "Đánh giá không thành công";
            return RedirectToAction("OrderDetails", "Customer", new { orderId = orderId });
        }
    }
}
