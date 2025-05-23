using System.Drawing;
using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Customer,Lessor")]
    public partial class CustomerController : BaseController
    {
        public IActionResult ManageOrder()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var ordersGrouped = _facadeService.Order.GetOrdersGroupedByStore(userId);

            var pendingOr = ordersGrouped
                .Where(o =>
                    o.Order.Status == OrderStatus.Pending
                    || o.Order.Status == OrderStatus.Renting
                    || o.Order.Status == OrderStatus.Delivering
                    || o.Order.Status == OrderStatus.Returning
                )
                .ToList();
            var completedOr = ordersGrouped
                .Where(o => o.Order.Status == OrderStatus.Completed)
                .ToList();
            var cancelOr = ordersGrouped
                .Where(o => o.Order.Status == OrderStatus.Cancelled)
                .ToList();

            ViewBag.Pending = pendingOr;
            ViewBag.Completed = completedOr;
            ViewBag.Cancelled = cancelOr;
            ViewBag.OrdersGrouped = ordersGrouped;

            return View();
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId, string reason)
        {
            var result = _facadeService.Order.CancelOrder(orderId, reason);
            if (!result)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("ManageOrder", "Customer");
            }
            TempData["success"] = "Hủy đơn hàng thành công";
            return RedirectToAction("OrderDetails", "Customer", new { orderId });
        }

        [HttpPost]
        public IActionResult MarkAsRenting(int orderId)
        {
            var result = _facadeService.Order.MarkAsRenting(orderId);
            if (!result)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("ManageOrder", "Customer");
            }
            TempData["success"] = "Cập nhật trạng thái đơn hàng thành công";
            return RedirectToAction("OrderDetails", "Customer", new { orderId });
        }

        [HttpPost]
        public IActionResult MarkAsReturning(int orderId)
        {
            var result = _facadeService.Order.MarkAsReturning(orderId);
            if (!result)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("ManageOrder", "Customer");
            }
            TempData["success"] =
                "Cập nhật trạng thái đơn hàng thành công, chúng tôi sẽ đến lấy đơn hàng trong 2-3 tiếng tới!";
            return RedirectToAction("OrderDetails", "Customer", new { orderId });
        }

        public IActionResult OrderDetails(int orderId)
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Home");
            }
            var user = _facadeService.User.GetUserByEmail(email);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var result = _facadeService.Order.GetOrderDetail(orderId, user.Id);

            if (result == null)
            {
                return View("Không tìm thấy đơn hàng.");
            }

            if (email != null)
            {
                if (user == null)
                {
                    TempData["error"] = "Không tìm thấy người dùng";
                }
                else
                {
                    ViewBag.UserName = user.UserName;
                }
            }

            ViewBag.CheckReview = result.Value.checkReview;
            ViewBag.GroupedByStore = result.Value.GroupedByStore;

            return View(result.Value.Order);
        }
    }
}
