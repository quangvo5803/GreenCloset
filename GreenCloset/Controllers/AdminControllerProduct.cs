using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult ManageProduct()
        {
            return View();
        }

        public IActionResult GetAllProduct()
        {
            var products = _facadeService
                .Product.GetAllProducts(includeProperties: "Categories,Feedbacks")
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Categories,
                    AvgRating = p.Feedbacks != null && p.Feedbacks.Any()
                        ? p.Feedbacks.Average(f => f.FeedbackStars)
                        : 0,
                    FeedbackCount = p.Feedbacks != null && p.Feedbacks.Any()
                        ? p.Feedbacks.Count()
                        : 0,
                })
                .ToList();
            ;
            return Json(new { data = products });
        }

        public IActionResult CreateProduct()
        {
            ViewBag.Role = User.FindFirstValue(ClaimTypes.Role);
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View();
        }

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
                TempData["success"] = "Tạo sản phẩm thành công";
                return RedirectToAction("ManageProduct");
            }
            TempData["error"] = "Tạo sản phẩm không thành công";
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View(product);
        }

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
                return RedirectToAction("ManageProduct");
            }
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            return View(product);
        }

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
                    return RedirectToAction("ManageProduct");
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
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("ManageProduct");
            }
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            TempData["error"] = "Cập nhật sản phẩm không thành công";
            return View(product);
        }

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

        [HttpDelete]
        public async Task<IActionResult> DeleteImageProduct(int imageId)
        {
            var image = _facadeService.ItemImage.GetItemImageById(imageId);
            if (image == null)
            {
                return Ok(new { success = false, message = "Không tìm thấy ảnh" });
            }

            await _facadeService.ItemImage.RemoveItemImage(image);
            return Ok(new { success = true });
        }

        public IActionResult ViewFeedbackProduct(int id)
        {
            var (product, feedbacks) = _facadeService.FeedBack.ViewFeedbackProduct(id);
            if (product == null)
            {
                TempData["error"] = "Không có Product";
                return RedirectToAction("ManageProduct");
            }

            ViewBag.Product = product;
            return View(feedbacks);
        }
    }
}
