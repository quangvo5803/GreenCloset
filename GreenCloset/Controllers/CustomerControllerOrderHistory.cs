using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace GreenCloset.Controllers
{
    [Authorize]
    [Authorize(Roles = "Customer")]
    public partial class CustomerController : BaseController
    {
        public IActionResult ManageOrder()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var ordersGrouped = _facadeService.OrderHistory.GetOrdersGroupedByStore(userId);

            var pendingOr = ordersGrouped.Where(o => o.Order.Status == OrderStatus.Pending).ToList();
            var deliveringOr = ordersGrouped.Where(o => o.Order.Status == OrderStatus.Delivering).ToList();
            var completedOr = ordersGrouped.Where(o => o.Order.Status == OrderStatus.Completed).ToList();
            var cancelOr = ordersGrouped.Where(o => o.Order.Status == OrderStatus.Cancelled).ToList();


            ViewBag.Pending = pendingOr;
            ViewBag.Delivering = deliveringOr;
            ViewBag.Completed = completedOr;
            ViewBag.Cancelled = cancelOr;
            ViewBag.OrdersGrouped = ordersGrouped;

            return View();
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }
            _facadeService.OrderHistory.CancelOrder(orderId, userId);
            return RedirectToAction("ManageOrder", "Customer");
        }

        public IActionResult OrderDetails(int orderId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return RedirectToAction("Login", "Home");
            }

            var result = _facadeService.OrderHistory.GetOrderDetail(orderId, userId);
            if (result == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

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
                    ViewBag.UserName = user.UserName;
                }
            }

            ViewBag.Order = result.Value.Order;
            ViewBag.GroupedByStore = result.Value.GroupedByStore;

            return View();
        }
    }
}
