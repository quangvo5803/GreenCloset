using BussinessLayer.Implement;
using BussinessLayer.Interface;
using DataAccess.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Repository.Implement;
using Repository.Interface;
using Utility.Email;
using Utility.Media;

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
                    options.LoginPath = "/Home/Login";
                    options.LogoutPath = "/Home/Logout";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                })
                //Login with Google
                .AddGoogle(
                    GoogleDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.ClientId = builder.Configuration["GoogleKeys:ClientID"];
                        options.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
                        options.Events.OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/Home/Login");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        };
                    }
                )
                .AddFacebook(
                    FacebookDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.AppId = builder.Configuration["FacebookKeys:AppID"];
                        options.AppSecret = builder.Configuration["FacebookKeys:AppSecret"];
                        options.Events.OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/Home/Login");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        };
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
            builder.Services.AddScoped<IVnPayService, VnPayService>();
            builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
            builder.Services.AddSingleton<EmailSender>();
            builder.Services.AddHostedService<BackgroundEmailSender>();
            builder.Services.AddSingleton<CloudinaryService>();
            var app = builder.Build();

            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

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
