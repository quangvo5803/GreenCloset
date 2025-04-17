using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IUserService
    {
        User? GetUserByEmail(string email);
        void UpdateUser(User user);
        Task<bool> Login(HttpContext httpContext, string email, string password);
        Task<bool> LoginWithGoogle(HttpContext httpContext, ClaimsPrincipal principal);
        bool Register(string email, string password);
        void SendEmailComfirm(string email);

        bool ConfirmEmail(string token);
        bool ChangePassword(string email, string oldPassword, string newPassword);
        bool ResetPassword(string email, string password);
        bool IsValidPassword(string password);
        void SendResetPasswordEmail(string email);
        string? IsValidToken(string token);
    }
}
