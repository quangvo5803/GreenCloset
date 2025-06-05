using System.Security.Claims;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IUserService
    {
        User? GetUserByEmail(string email);
        void UpdateUser(User user);
        Task<User?> Login(HttpContext httpContext, string email, string password);
        Task<User?> LoginWithGoogle(HttpContext httpContext, ClaimsPrincipal principal);
        Task<User?> LoginWithFacebook(HttpContext httpContext, ClaimsPrincipal principal);
        bool Register(string email, string password);
        void SendEmailComfirm(string email);

        bool ConfirmEmail(string token);
        bool ChangePassword(string email, string oldPassword, string newPassword);
        bool ResetPassword(string email, string password);
        bool IsValidPassword(string password);
        void SendResetPasswordEmail(string email);
        string? IsValidToken(string token);
        void RegisterLessor(string email, string storeName, string phoneNumber, string address);
        void Contact(string name, string email, string message);
        IEnumerable<User> GetAllCustomer();
        IEnumerable<User> GetAllLessor();
        Task SignInUser(HttpContext httpContext, User user);
        Task SubmitBillLessoer(Guid userId, IFormFile file);
        bool UpdateMonthlyFeeAdmin(Guid userId, bool isPaid);
    }
}
