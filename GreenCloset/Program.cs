using BussinessLayer.Implement;
using BussinessLayer.Interface;
using DataAccess.Data;
using GreenCloset.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Repository.Implement;
using Repository.Interface;

namespace GreenCloset
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder
                .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login";
                    options.LogoutPath = "/User/Logout";
                    options.AccessDeniedPath = "/User/AccessDenied";
                })
                //Login with Google
                .AddGoogle(
                    GoogleDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ClientId = builder.Configuration["GoogleKeys:ClientID"];
                        options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
                    }
                );
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddAuthorization();

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IFacedeService, FacadeService>();

            builder.Services.AddSingleton<EmailSender>();
            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            app.Run();
        }
    }
}
