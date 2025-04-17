using System.Security.Claims;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(IFacedeService facadeService)
            : base(facadeService) { }

        public IActionResult Profile()
        {
            var emailUser = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (emailUser == null)
            {
                return NotFound();
            }
            var user = _facadeService.User.GetUserByEmail(emailUser);
            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(User user)
        {
            if (ModelState.IsValid)
            {
                _facadeService.User.UpdateUser(user);
                TempData["success"] = "Cập nhật thông tin thành công";
            }
            TempData["error"] = "Cập nhật thông tin không thành công";
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(
            string oldpassword,
            string newpassword,
            string repassword
        )
        {
            if (newpassword != repassword)
            {
                TempData["error"] = "Mật khẩu không khớp";
                return View();
            }
            if (!_facadeService.User.IsValidPassword(newpassword))
            {
                TempData["error"] = "Mật khẩu mới không hợp lệ";
                return View();
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (
                email == null
                || !_facadeService.User.ChangePassword(email, oldpassword, newpassword)
            )
            {
                TempData["error"] = "Mật khẩu cũ không đúng";
                return View();
            }
            TempData["success"] = "Đổi mật khẩu thành công";
            return View();
        }

        [HttpGet]
        public IActionResult ManageOrder()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }
    }
}
