using DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public partial class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _facadeService.User.Login(HttpContext, email, password);
            if (user == null)
            {
                TempData["error"] = "Tài khoản hoặc mật khẩu không đúng!";
                return View();
            }
            if (user != null && !user.IsEmailConfirmed)
            {
                TempData["error"] = "Vui lòng xác thực email trước khi đang nhập!";
                return View();
            }
            TempData["success"] = "Đăng nhập thành công";
            return user!.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Customer => RedirectToAction("Index", "Home"),
                UserRole.Lessor => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [HttpGet("login-google")]
        public async Task LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var authResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            if (!authResult.Succeeded || authResult.Principal == null)
            {
                TempData["error"] = "Đăng nhập không thành công";
                return RedirectToAction("Login", "Home");
            }
            var user = await _facadeService.User.LoginWithGoogle(HttpContext, authResult.Principal);
            if (user == null)
            {
                TempData["error"] = "Đăng nhập không thành công";
                return RedirectToAction("Login", "Home");
            }
            TempData["success"] = "Đăng nhập thành công";
            return user!.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Customer => RedirectToAction("Index", "Home"),
                UserRole.Lessor => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        public async Task LoginWithFacebook()
        {
            var redirectUrl = Url.Action("FacebookResponse", "Home");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, properties);
        }

        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            if (!result.Succeeded || result.Principal == null)
            {
                TempData["error"] = "Đăng nhập không thành công";

                return RedirectToAction("Login", "Home");
            }

            var user = await _facadeService.User.LoginWithFacebook(HttpContext, result.Principal);
            if (user == null)
            {
                TempData["error"] = "Đăng nhập không thành công";
                return RedirectToAction("Login", "Home");
            }
            TempData["success"] = "Đăng nhập thành công";
            return user!.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Admin"),
                UserRole.Customer => RedirectToAction("Index", "Home"),
                UserRole.Lessor => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        [HttpGet]
        [HttpGet]
        public IActionResult Register(string? email)
        {
            ViewBag.Email = email;
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
            if (!_facadeService.User.IsValidPassword(password))
            {
                TempData["error"] = "Mật khẩu không hợp lệ";
                ViewBag.Email = email;
                return View();
            }
            if (!_facadeService.User.Register(email, password))
            {
                TempData["error"] = "Email đã có người sử dụng";
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
                return RedirectToAction("Login", "Home");
            }
            TempData["success"] = "Xác thực email thành công";
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (_facadeService.User.GetUserByEmail(email) != null)
            {
                _facadeService.User.SendResetPasswordEmail(email);
            }
            TempData["success"] = "Vui lòng kiểm tra email để đặt lại mật khẩu";
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var email = _facadeService.User.IsValidToken(token);
            if (string.IsNullOrEmpty(token) || email == null)
            {
                TempData["error"] = "Token không hợp lệ";
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string password, string repassword)
        {
            if (password != repassword)
            {
                TempData["error"] = "Mật khẩu không khớp";
                return View();
            }
            if (!_facadeService.User.IsValidPassword(password))
            {
                TempData["error"] = "Mật khẩu mới không hợp lệ";
                return View();
            }
            var success = _facadeService.User.ResetPassword(email, password);
            if (!success)
            {
                TempData["error"] = "Đặt lại mật khẩu không thành công";
                return View();
            }
            TempData["success"] = "Đặt lại mật khẩu thành công";
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResendEmailConfirmation(string email)
        {
            if (_facadeService.User.GetUserByEmail(email) != null)
            {
                _facadeService.User.SendEmailComfirm(email);
            }
            TempData["success"] = "Vui lòng kiểm tra email để xác thực tài khoản";
            return RedirectToAction("Login", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }
    }
}
