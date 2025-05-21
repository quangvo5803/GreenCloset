using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Repository.Implement;
using Utility.Email;
using Utility.Password;

namespace BussinessLayer.Implement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailQueue _emailQueue;

        public UserService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IEmailQueue emailQueue
        )
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _emailQueue = emailQueue;
        }

        public User? GetUserByEmail(string email)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            return user;
        }

        public void UpdateUser(User user)
        {
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
        }

        public async Task<User?> Login(HttpContext httpContext, string email, string password)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            await SignInUser(httpContext, user);
            return user;
        }

        public async Task<User?> LoginWithGoogle(HttpContext httpContex, ClaimsPrincipal principal)
        {
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
                return null;

            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = name,
                    PasswordHash = PasswordHasher.HashPassword("Abc123@"),
                    Role = UserRole.Customer,
                    IsEmailConfirmed = true,
                };
                _unitOfWork.User.Add(user);
                _unitOfWork.Save();
                string subject = "Xác nhận đăng kí tài khoản GreenCloset";
                string body = GetComfirmationEmailGoogle();
                _emailQueue.QueueEmail(email, subject, body);
            }
            await SignInUser(httpContex, user);
            return user;
        }

        public async Task<User?> LoginWithFacebook(
            HttpContext httpContex,
            ClaimsPrincipal principal
        )
        {
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name =
                principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? principal.Identity?.Name;

            if (string.IsNullOrEmpty(email))
                return null;

            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = name ?? email.Split('@')[0],
                    PasswordHash = PasswordHasher.HashPassword("Abc123@"),
                    Role = UserRole.Customer,
                    IsEmailConfirmed = true,
                };
                _unitOfWork.User.Add(user);
                _unitOfWork.Save();

                string subject = "Xác nhận đăng kí tài khoản GreenCloset";
                string body = GetComfirmationEmailGoogle();

                _emailQueue.QueueEmail(email, subject, body);
            }

            await SignInUser(httpContex, user);
            return user;
        }

        public bool Register(string email, string password)
        {
            if (_unitOfWork.User.Get(u => u.Email == email) != null)
            {
                return false;
            }
            string hashedPassword = PasswordHasher.HashPassword(password);
            var user = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                Role = UserRole.Customer,
                IsEmailConfirmed = false,
            };
            _unitOfWork.User.Add(user);
            _unitOfWork.Save();
            //Tạo token
            SendEmailComfirm(email);
            return true;
        }

        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null || !PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
            {
                return false;
            }
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
            return true;
        }

        public bool ResetPassword(string email, string password)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                return false;
            }
            user.ComfirmationToken = null;
            user.PasswordHash = PasswordHasher.HashPassword(password);
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
            return true;
        }

        public void SendEmailComfirm(string email)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user != null)
            {
                var comformationToken = Guid.NewGuid().ToString();
                var token = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{email}:{comformationToken}")
                );
                user.ComfirmationToken = comformationToken;
                _unitOfWork.User.Update(user);
                _unitOfWork.Save();
                //Tạo link xác nhận
                string confirmUrl =
                    $"{_configuration["AppSettings:BaseUrl"]}/Home/ConfirmEmail?token={token}";
                string subject = "Xác nhận đăng kí tài khoản Green Closet";
                string body = GetComfirmationEmail(confirmUrl);
                _emailQueue.QueueEmail(email, subject, body);
            }
        }

        public bool ConfirmEmail(string token)
        {
            var email = IsValidToken(token);
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                return false;
            }
            user.ComfirmationToken = null;
            user.IsEmailConfirmed = true;
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
            return true;
        }

        public async Task SignInUser(HttpContext httpContext, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );
        }

        public void SendResetPasswordEmail(string email)
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                return;
            }
            var comfirmationToken = Guid.NewGuid().ToString();
            user.ComfirmationToken = comfirmationToken;
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
            var token = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{email}:{comfirmationToken}")
            );
            string confirmUrl =
                $"{_configuration["AppSettings:BaseUrl"]}/Home/ResetPassword?token={token}";
            string subject = "Đặt lại mật khẩu tài khoản GreenCloset";
            string body = GetForgotPasswordEmail(confirmUrl);
            _emailQueue.QueueEmail(email, subject, body);
        }

        public string? IsValidToken(string token)
        {
            var decodedBytes = Convert.FromBase64String(token);
            var decodedString = Encoding.UTF8.GetString(decodedBytes);
            var email = decodedString.Split(':')[0];
            var comfirmationToken = decodedString.Split(':')[1];
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null || user.ComfirmationToken != comfirmationToken)
            {
                return null;
            }
            return email;
        }

        public bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
        }

        public void RegisterLessor(
            string email,
            string storeName,
            string phoneNumber,
            string adresss
        )
        {
            var user = _unitOfWork.User.Get(u => u.Email == email);
            if (user == null)
            {
                return;
            }
            user.Role = UserRole.Lessor;
            user.ShopName = storeName;
            user.PhoneNumber = phoneNumber;
            user.Address = adresss;
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
        }

        private string GetComfirmationEmail(string confirmUrl)
        {
            return $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">"
                + $"<tr>"
                + $"<td></td>"
                + $"<td class=\"container\" style=\"margin: 0 auto !important; max-width: 600px; padding: 0; padding-top: 24px; width: 600px;\">"
                + $"<div class=\"content\" style=\"box-sizing: border-box; display: block; margin: 0 auto; max-width: 600px; padding: 0;\">"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"main\" style=\"background: #f0f8f0; border: 1px solid #2e7d32; border-radius: 16px; width: 100%; text-align: center;\">"
                + $"<tr>"
                + $"<td class=\"wrapper\" style=\"box-sizing: border-box; padding: 24px;\">"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Chào bạn</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Chúng tôi cần xác thực email của bạn trước khi hoàn tất đăng kí tài khoản. Vui lòng bấm vào nút bên dưới.</p>"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"min-width: 100% !important; width: 100%;\">"
                + $"<tbody>"
                + $"<tr>"
                + $"<td align=\"center\" style=\"padding-bottom: 16px;\">"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">"
                + $"<tbody>"
                + $"<tr>"
                + $"<td><a href='{confirmUrl}' style=\"background-color: #2e7d32; border: solid 2px #2e7d32; border-radius: 4px; box-sizing: border-box; color: #ffffff; cursor: pointer; display: inline-block; font-size: 16px; font-weight: bold; margin: 0; padding: 12px 24px; text-decoration: none; text-transform: capitalize;\">Bấm vào đây</a></td>"
                + $"</tr>"
                + $"</tbody>"
                + $"</table>"
                + $"</td>"
                + $"</tr>"
                + $"</tbody>"
                + $"</table>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Cảm ơn bạn đã tin tưởng Green Closet</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Green Closet</p>"
                + $"<div style=\"text-align: center; margin-top: 20px;\">"
                + $"<img src=\"https://i.imgur.com/0Iphozz.png\" alt=\"Green Closet Logo\" style=\"max-width: 200px; height: auto; border-radius: 8px;\">"
                + $"</div>"
                + $"</td>"
                + $"</tr>"
                + $"</table>"
                + $"</div>"
                + $"</td>"
                + $"<td></td>"
                + $"</tr>"
                + $"</table>";
        }

        private string GetComfirmationEmailGoogle()
        {
            return $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">"
                + $"<tr>"
                + $"<td></td>"
                + $"<td class=\"container\" style=\"margin: 0 auto !important; max-width: 600px; padding: 0; padding-top: 24px; width: 600px;\">"
                + $"<div class=\"content\" style=\"box-sizing: border-box; display: block; margin: 0 auto; max-width: 600px; padding: 0;\">"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"main\" style=\"background: #f0f8f0; border: 1px solid #2e7d32; border-radius: 16px; width: 100%; text-align: center;\">"
                + $"<tr>"
                + $"<td class=\"wrapper\" style=\"box-sizing: border-box; padding: 24px;\">"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Chào bạn</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Chúc mừng bạn đã đăng ký tài khoản thành công tại Green Closet!</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Mật khẩu mặc định của bạn là: <strong>Abc123@</strong></p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Vui lòng đổi mật khẩu ngay sau khi đăng nhập để bảo mật tài khoản của bạn.</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Cảm ơn bạn đã tin tưởng Green Closet</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Green Closet</p>"
                + $"<div style=\"text-align: center; margin-top: 20px;\">"
                + $"<img src=\"https://i.imgur.com/0Iphozz.png\" alt=\"Green Closet Logo\" style=\"max-width: 200px; height: auto; border-radius: 8px;\">"
                + $"</div>"
                + $"</td>"
                + $"</tr>"
                + $"</table>"
                + $"</div>"
                + $"</td>"
                + $"<td></td>"
                + $"</tr>"
                + $"</table>";
        }

        private string GetForgotPasswordEmail(string confirmUrl)
        {
            return $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">"
                + $"<tr>"
                + $"<td></td>"
                + $"<td class=\"container\" style=\"margin: 0 auto !important; max-width: 600px; padding: 0; padding-top: 24px; width: 600px;\">"
                + $"<div class=\"content\" style=\"box-sizing: border-box; display: block; margin: 0 auto; max-width: 600px; padding: 0;\">"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"main\" style=\"background: #f0f8f0; border: 1px solid #2e7d32; border-radius: 16px; width: 100%; text-align: center;\">"
                + $"<tr>"
                + $"<td class=\"wrapper\" style=\"box-sizing: border-box; padding: 24px;\">"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Chào bạn</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #1b5e20;\">Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Green Closet của mình. Nhấp vào nút bên dưới để đặt lại mật khẩu của bạn.</p>"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"min-width: 100% !important; width: 100%;\">"
                + $"<tbody>"
                + $"<tr>"
                + $"<td align=\"center\" style=\"padding-bottom: 16px;\">"
                + $"<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">"
                + $"<tbody>"
                + $"<tr>"
                + $"<td><a href='{confirmUrl}' style=\"background-color: #2e7d32; border: solid 2px #2e7d32; border-radius: 4px; box-sizing: border-box; color: #ffffff; cursor: pointer; display: inline-block; font-size: 16px; font-weight: bold; margin: 0; padding: 12px 24px; text-decoration: none; text-transform: capitalize;\">Đặt lại mật khẩu</a></td>"
                + $"</tr>"
                + $"</tbody>"
                + $"</table>"
                + $"</td>"
                + $"</tr>"
                + $"</tbody>"
                + $"</table>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #bf360c;\">Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này hoặc liên hệ với nhóm hỗ trợ của chúng tôi nếu bạn có bất kỳ thắc mắc nào.</p>"
                + $"<p style=\"font-weight: normal; margin: 0; margin-bottom: 16px; color: #bf360c;\">Green Closet</p>"
                + $"<div style=\"text-align: center; margin-top: 20px;\">"
                + $"<img src=\"https://i.imgur.com/0Iphozz.png\" alt=\"Green Closet Logo\" style=\"max-width: 200px; height: auto; border-radius: 8px;\">"
                + $"</div>"
                + $"</td>"
                + $"</tr>"
                + $"</table>"
                + $"</div>"
                + $"</td>"
                + $"<td></td>"
                + $"</tr>"
                + $"</table>";
        }
    }
}
