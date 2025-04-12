using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var products = _facedeService
                .Product.GetAllProducts(includeProperties: "Categories")
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

        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _facedeService.Category.GetAllCategories();
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        )
        {
            if (ModelState.IsValid)
            {
                _facedeService.Product.AddProduct(product, selectedCategories, avatar, gallery);
                TempData["success"] = "Tạo sản phẩm thành công";
                return RedirectToAction("ManageProduct");
            }
            TempData["error"] = "Tạo sản phẩm không thành công";
            ViewBag.Categories = _facedeService.Category.GetAllCategories();
            return View(product);
        }

        public IActionResult UpdateProduct(int id)
        {
            var product = _facedeService.Product.GetProductById(
                id,
                includeProperties: "Categories,ProductAvatar,ProductImages,Feedbacks"
            );
            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("ManageProduct");
            }
            ViewBag.Categories = _facedeService.Category.GetAllCategories();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        )
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _facedeService.Product.GetProductById(
                    product.Id,
                    includeProperties: "Categories,ProductAvatar,ProductImages"
                );

                if (existingProduct == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("ManageProduct");
                }
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                await _facedeService.Product.UpdateProduct(
                    existingProduct,
                    selectedCategories,
                    avatar,
                    gallery
                );
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("ManageProduct");
            }
            ViewBag.Categories = _facedeService.Category.GetAllCategories();
            TempData["error"] = "Cập nhật sản phẩm không thành công";
            return View(product);
        }

        [HttpDelete]
        public IActionResult DeleteProduct(int id)
        {
            var product = _facedeService.Product.GetProductById(
                id,
                includeProperties: "Categories,ProductAvatar,ProductImages"
            );
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }
            bool result = _facedeService.Product.DeleteProduct(product);
            if (result)
            {
                return Json(new { success = true, message = "Xóa sản phẩm thành công" });
            }
            return Json(new { success = false, message = "Xóa sản phẩm không thành công" });
        }

        [HttpDelete]
        public IActionResult DeleteImageProduct(int imageId)
        {
            var image = _facedeService.ItemImage.GetItemImageById(imageId);
            if (image == null)
            {
                return Ok(new { success = false, message = "Không tìm thấy ảnh" });
            }

            _facedeService.ItemImage.RemoveItemImage(image);
            return Ok(new { success = true });
        }
    }
}
