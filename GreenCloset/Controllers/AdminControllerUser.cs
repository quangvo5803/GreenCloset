using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public partial class AdminController : Controller
    {
        public IActionResult ManageUser()
        {
            return View();
        }

        public IActionResult GetAllCustomer()
        {
            var customers = _facadeService
                .User.GetAllCustomer()
                .Select(l => new
                {
                    l.Id,
                    l.UserName,
                    l.Email,
                    l.PhoneNumber,
                    RentalOrderCount = _facadeService
                        .Order.GetOrdersGroupedByStore(l.Id)
                        .Where(o => o.Order.Status == OrderStatus.Completed)
                        .Count(),
                    TotalRentalMoney = _facadeService
                        .Order.GetOrdersGroupedByStore(l.Id)
                        .Where(o => o.Order.Status == OrderStatus.Completed)
                        .Sum(o => o.Order.TotalPrice),
                })
                .ToList();
            ;
            return Json(new { data = customers });
        }

        public IActionResult GetAllLessor()
        {
            var lessors = _facadeService
                .User.GetAllLessor()
                .Select(l => new
                {
                    l.Id,
                    l.Email,
                    l.ShopName,
                    l.PhoneNumber,
                    ProductCount = _facadeService
                        .Product.GetLessorProducts(l.Email)
                        .Where(p => p.Available)
                        .Count(),
                    OrderCount = _facadeService
                        .Order.GetOrdersByProductOwner(l.Email)
                        .Where(o => o.Status == OrderStatus.Completed)
                        .Count(),
                    AverageFeeback = _facadeService.FeedBack.GetAllShopFeedback(l.Id).Any()
                        ? _facadeService
                            .FeedBack.GetAllShopFeedback(l.Id)
                            .Average(f => f.FeedbackStars)
                        : 0,
                    FeedbackCount = _facadeService.FeedBack.GetAllShopFeedback(l.Id).Count(),
                })
                .ToList();
            return Json(new { data = lessors });
        }
    }
}
