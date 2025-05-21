using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public partial class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.TotalProduct = _facadeService
                .Product.GetLessorProducts()
                .Where(p => p.Available)
                .Count();
            ViewBag.OrderPending = _facadeService
                .Order.GetOrdersByProductOwner()
                .Where(o => o.Status == OrderStatus.Pending)
                .Count();
            ViewBag.OrderComplete = _facadeService
                .Order.GetOrdersByProductOwner()
                .Where(o => o.Status == OrderStatus.Completed)
                .Count();
            ViewBag.Revenue = _facadeService
                .Order.GetOrdersByProductOwner()
                .Where(o => o.Status == OrderStatus.Completed)
                .Sum(o => o.TotalPrice);
            var today = DateTime.Today;
            var currentDayOfWeek = (int)today.DayOfWeek;
            var monday = today.AddDays(currentDayOfWeek == 0 ? -6 : -(currentDayOfWeek - 1));

            var allOrders = _facadeService.Order.GetOrdersByProductOwner().ToList();

            var revenueByDay = new List<decimal>();
            double weekRevenue = 0;
            int weekOrder = 0;
            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);
                var dailyOrders = allOrders.Where(o => o.OrderDate.Date == date).ToList();
                var dailyRevenue = dailyOrders.Sum(o => o.TotalPrice);
                var inMillions = Math.Round((decimal)dailyRevenue / 1000000, 2);
                weekRevenue += dailyRevenue;
                weekOrder += dailyOrders.Count();
                revenueByDay.Add(inMillions);
            }
            ViewBag.WeeklyOrder = weekOrder;
            ViewBag.WeeklyRevenue = weekRevenue;
            return View();
        }

        [HttpGet]
        public IActionResult GetMonthlyProfit(int year)
        {
            var orderList = _facadeService
                .Order.GetOrdersByProductOwner()
                .Where(o => o.OrderDate.Year == year);
            Console.WriteLine(orderList.Count());
            var monthlyRevenue = new decimal[12];
            var monthlyOrderCount = new int[12];
            var monthlyProductRentCount = new int[12];
            var monthlyUniqueUserSet = new HashSet<string>[12];
            for (int i = 0; i < 12; i++)
            {
                monthlyUniqueUserSet[i] = new HashSet<string>();
            }
            foreach (var order in orderList)
            {
                var month = order.OrderDate.Month - 1;
                monthlyRevenue[month] += Math.Round((decimal)order.TotalPrice / 1000000, 2);
                monthlyOrderCount[month]++;
                monthlyUniqueUserSet[month].Add(order.UserId.ToString());
                if (order.OrderDetails != null)
                {
                    monthlyProductRentCount[month] += order.OrderDetails.Count;
                }
            }

            var result = new
            {
                Months = new[]
                {
                    "Tháng 1",
                    "Tháng 2",
                    "Tháng 3",
                    "Tháng 4",
                    "Tháng 5",
                    "Tháng 6",
                    "Tháng 7",
                    "Tháng 8",
                    "Tháng 9",
                    "Tháng 10",
                    "Tháng 11",
                    "Tháng 12",
                },
                MonthlyRevenues = monthlyRevenue,
                MonthlyOrderCounts = monthlyOrderCount,
                MonthlyProductRentCounts = monthlyProductRentCount,
                MonthlyUniqueCustomerCounts = monthlyUniqueUserSet.Select(s => s.Count).ToArray(),
            };

            return Json(result);
        }

        [HttpGet]
        public JsonResult GetWeeklyRevenue()
        {
            var today = DateTime.Today;
            var currentDayOfWeek = (int)today.DayOfWeek;
            var monday = today.AddDays(currentDayOfWeek == 0 ? -6 : -(currentDayOfWeek - 1));

            var allOrders = _facadeService.Order.GetOrdersByProductOwner().ToList();

            var revenueByDay = new List<double>();
            double weekRevenue = 0;
            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);
                var dailyOrders = allOrders.Where(o => o.OrderDate.Date == date).ToList();
                var dailyRevenue = dailyOrders.Sum(o => o.TotalPrice);
                weekRevenue += dailyRevenue;

                revenueByDay.Add(dailyRevenue);
            }
            ViewBag.WeeklyRevenue = weekRevenue;
            return Json(revenueByDay);
        }
    }
}
