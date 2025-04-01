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
    public class UserController : Controller
    {
        private IFacedeService _facadeService;

        public UserController(IFacedeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var success = await _facadeService.User.Login(HttpContext, email, password);
            if (!success)
            {
                TempData["error"] = "Email hoặc mật khẩu không đúng";
                return View();
            }
            var user = _facadeService.User.GetUserByEmail(email);
            if (user == null || !user.IsEmailConfirmed)
            {
                {
                    TempData["error"] = "Vui lòng xác thực email trước khi đang nhập";
                    return View();
                }
            }
            return user!.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Customer => RedirectToAction("Index", "Home"),
                UserRole.Lessor => RedirectToAction("Index", "Lessor"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [HttpGet("login-google")]
        public async Task LoginWithGoogle()
        {
            await HttpContext.ChallengeAsync(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") }
            );
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var authResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                return RedirectToAction("Login", "User");
            }
            var success = await _facadeService.User.LoginWithGoogle(
                HttpContext,
                authResult.Principal
            );
            if (!success)
            {
                TempData["error"] = "Đăng nhập không thành công";
                return RedirectToAction("Login", "User");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string password, string repassword)
        {
            if (password != repassword)
            {
                TempData["error"] = "Mật khẩu không khớp.";
                ViewBag.Email = email;
                return View();
            }
            if (_facadeService.User.IsValidPassword(password))
            {
                TempData["error"] = "Mật khẩu không hợp lệ";
                ViewBag.Email = email;
                return View();
            }
            if (!_facadeService.User.Register(email, password))
            {
                TempData["error"] = "Email đã có người sử dụng hoặc mật khẩu không hợp lệ";
                ViewBag.Email = email;
                return View();
            }
            TempData["success"] = "Đăng kí thành công! Vui lòng xác nhận email trước khi đăng nhập";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string token)
        {
            if (!_facadeService.User.ConfirmEmail(token))
            {
                TempData["error"] = "Token xác thực không hợp lệ";
                return RedirectToAction("Login", "User");
            }
            TempData["success"] = "Xác thực email thành công";
            return RedirectToAction("Login", "User");
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(
            string oldPassword,
            string newPassword,
            string rePassword
        )
        {
            if (newPassword != rePassword)
            {
                TempData["error"] = "Mật khẩu không khớp";
                return View();
            }
            var email = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (
                email == null
                || !_facadeService.User.ChangePassword(email, oldPassword, newPassword)
            )
            {
                TempData["error"] = "Mật khẩu cũ không đúng";
                return View();
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }
    }
}
