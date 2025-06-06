using BussinessLayer.Implement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult ManageOrder()
        {
            var rsOr = _facadeService.Order.GetAllOrAdmin();
            return View(rsOr);
        }

        public IActionResult OrderDetails(int orderId)
        {
            var rsOrDetail = _facadeService.Order.GetOrderDetailsAdmin(orderId);
            if (rsOrDetail == null)
            {
                TempData["error"] = "Xem chi tiết không thành công";
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.GroupedByStore = rsOrDetail.Value.GroupedByStore;
            return View(rsOrDetail.Value.Order);
        }
    }
}
