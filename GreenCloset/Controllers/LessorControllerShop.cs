using System.Security.Claims;
using System.Threading.Tasks;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public partial class LessorController : BaseController
    {
        public LessorController(IFacedeService facedeService)
            : base(facedeService) { }

        [Authorize(Roles = "Customer,Lessor")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> RegisterLessor(
            string storeName,
            string phoneNumber,
            string address
        )
        {
            var emailUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailUser))
            {
                TempData["error"] = "Có lỗi xảy ra, vui lòng thử lại sau";
                return RedirectToAction("Index", "Lessor");
            }

            _facadeService.User.RegisterLessor(emailUser, storeName, phoneNumber, address);

            var user = _facadeService.User.GetUserByEmail(emailUser);

            if (user != null)
            {
                await HttpContext.SignOutAsync();

                await _facadeService.User.SignInUser(HttpContext, user);
            }

            TempData["success"] = "Đăng kí bán hàng thành công";
            return RedirectToAction("Index", "Lessor");
        }

        [Authorize(Roles = "Lessor")]
        [HttpGet]
        public IActionResult GetAllProduct()
        {
            var emailUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailUser))
            {
                return Json(new { data = new List<Product>() });
            }
            var products = _facadeService
                .Product.GetLessorProducts(emailUser, includeProperties: "Categories")
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Categories,
                    AvgRating = p.Feedbacks != null ? p.Feedbacks.Average(f => f.FeedbackStars) : 0,
                    FeedbackCount = p.Feedbacks != null ? p.Feedbacks.Count() : 0,
                })
                .ToList();
            ;
            return Json(new { data = products });
        }

        [Authorize(Roles = "Lessor")]
        [HttpGet]
        public IActionResult GetAllOrder()
        {
            var emailUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailUser))
            {
                return Json(new { data = new List<Product>() });
            }
            var orders = _facadeService
                .Order.GetOrdersByProductOwner(emailUser)
                .Select(p => new
                {
                    p.Id,
                    p.DeliveryOption,
                    ProductCount = p.OrderDetails?.Count,
                    p.TotalPrice,
                    p.Status,
                })
                .ToList();
            ;
            return Json(new { data = orders });
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult CreateProduct()
        {
            ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View();
        }

        [Authorize(Roles = "Lessor")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery,
            List<SizeClother> SelectedClotherSizes,
            List<int> SelectedShoeSizes
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid && userId != null)
            {
                product.SizeClother = SelectedClotherSizes;
                product.SizeShoe = SelectedShoeSizes;
                await _facadeService.Product.AddProduct(
                    Guid.Parse(userId),
                    product,
                    selectedCategories,
                    avatar,
                    gallery
                );
                return RedirectToAction("Index", "Lessor");
            }
            TempData["error"] = "Tạo sản phẩm không thành công";
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View(product);
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult UpdateProduct(int id)
        {
            ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);
            var product = _facadeService.Product.GetProductById(
                id,
                includeProperties: "Categories,ProductAvatar,ProductImages,Feedbacks"
            );
            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("Index", "Lessor");
            }
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View(product);
        }

        [Authorize(Roles = "Lessor")]
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery,
            List<SizeClother> SelectedClotherSizes,
            List<int> SelectedShoeSizes
        )
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _facadeService.Product.GetProductById(
                    product.Id,
                    includeProperties: "Categories,ProductAvatar,ProductImages"
                );

                if (existingProduct == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("Index", "Lessor");
                }
                existingProduct.Available = product.Available;
                existingProduct.SizeClother = SelectedClotherSizes;
                existingProduct.SizeShoe = SelectedShoeSizes;
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.DepositPrice = product.DepositPrice;
                existingProduct.Color = product.Color;
                await _facadeService.Product.UpdateProduct(
                    existingProduct,
                    selectedCategories,
                    avatar,
                    gallery
                );
                return RedirectToAction("Index", "Lessor");
            }
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            TempData["error"] = "Cập nhật sản phẩm không thành công";
            return View(product);
        }

        [Authorize(Roles = "Lessor")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _facadeService.Product.GetProductById(
                id,
                includeProperties: "Categories,ProductAvatar,ProductImages"
            );
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }
            bool result = await _facadeService.Product.DeleteProduct(product);
            if (result)
            {
                return Json(new { success = true, message = "Xóa sản phẩm thành công" });
            }
            return Json(new { success = false, message = "Xóa sản phẩm không thành công" });
        }

        [Authorize(Roles = "Lessor")]
        [HttpDelete]
        public async Task<IActionResult> DeleteImageProduct(int imageId)
        {
            var image = _facadeService.ItemImage.GetItemImageById(imageId);
            if (image == null)
            {
                return Ok(new { success = false, message = "Không tìm thấy ảnh" });
            }

            await _facadeService.ItemImage.RemoveItemImage(image);
            return Ok(new { successuccess = true });
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult OrderDetail(int orderId)
        {
            var order = _facadeService.Order.GetOrder(orderId);
            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("ManageOrder", "Customer");
            }
            return View(order);
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult MarkAsDelivered(int orderId)
        {
            var order = _facadeService.Order.MarkAsDelivered(orderId);
            if (!order)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Lessor");
            }
            TempData["success"] = "Cập nhật trạng thái đơn hàng thành công";
            return RedirectToAction("OrderDetail", "Lessor", new { orderId });
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult MarkAsComplete(int orderId)
        {
            var order = _facadeService.Order.CompleteOrder(orderId);
            if (!order)
            {
                TempData["error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "Lessor");
            }
            TempData["success"] = "Cập nhật trạng thái đơn hàng thành công";
            return RedirectToAction("OrderDetail", "Lessor", new { orderId });
        }

        [Authorize(Roles = "Lessor")]
        public IActionResult ViewFeedbackProducts(int id)
        {
            var (product, feedbacks) = _facadeService.FeedBack.ViewFeedbackProduct(id);
            if (product == null)
            {
                TempData["error"] = "Không có Product";
                return RedirectToAction("Index", "Lessor");
            }

            ViewBag.Product = product;
            return View(feedbacks);
        }
    }
}
