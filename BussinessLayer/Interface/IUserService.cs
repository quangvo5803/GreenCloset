using System.Security.Claims;
using GreenCloset.Models;
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

        bool ChangePassword(string email, string oldPassword, string newPassword);
    }
}
