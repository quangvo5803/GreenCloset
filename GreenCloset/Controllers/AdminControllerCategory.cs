using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class AdminController : Controller
    {
        private IFacedeService _facadeService;

        public AdminController(IFacedeService facedeService)
        {
            _facadeService = facedeService;
        }

        public IActionResult ManageCategory()
        {
            return View();
        }

        public IActionResult GetAllCategory()
        {
            var categories = _facadeService
                .Category.GetAllCategories(includeProperties: "Products")
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    ProductCount = c.Products?.Count(),
                })
                .ToList();
            ;
            return Json(new { data = categories });
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _facadeService.Category.AddCategory(category);
                TempData["success"] = "Tạo danh mục thành công";
                return RedirectToAction("ManageCategory");
            }
            TempData["error"] = "Tạo danh mục thất bại";
            return View(category);
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _facadeService.Category.GetCategoryById(id);
            if (category == null)
            {
                TempData["error"] = "Lỗi! Không tìm thấy danh mục";
                return RedirectToAction("ManageCategory");
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _facadeService.Category.UpdateCategory(category);
                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("ManageCategory");
            }
            TempData["error"] = "Cập nhật danh mục thất bại";
            return View(category);
        }

        [HttpDelete]
        public IActionResult DeleteCategory(int id)
        {
            var category = _facadeService.Category.GetCategoryById(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Lỗi! Không tìm thấy danh mục" });
            }
            _facadeService.Category.DeleteCategory(category);
            TempData["success"] = "Xóa danh mục thành công";
            return Json(new { success = true, message = "Xóa danh mục thành công" });
        }
    }
}
