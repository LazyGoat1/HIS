using System.Runtime.CompilerServices;
using HIS.Repository;
using HIS.Repository.Data;
using HIS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("HIS.Tests")]

namespace HIS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ====== 数据库上下文 ======
            builder.Services.AddDbContext<HisDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("HIS.Repository")));

            // ====== Cookie 认证 ======
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = ".HIS.Auth";
                });

            // ====== 分层依赖注入（由各层扩展方法统一注册） ======
            builder.Services.AddRepositoryServices();   // 仓储层
            builder.Services.AddBusinessServices();     // 服务层

            // ====== MVC 服务 ======
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                    // 允许字符串→数字转换：Layui 表单 data.field 的值全是字符串
                    // 不开启则 {"DepartmentId":"1"} 无法反序列化为 long DepartmentId
                    options.JsonSerializerOptions.NumberHandling =
                        System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
                });

            // ====== 基础设施 ======
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            // ====== 中间件管道 ======
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            // ====== 路由 ======
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ====== 数据库初始化（开发环境自动建库 + 种子数据） ======
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HisDbContext>();
                if (app.Environment.IsDevelopment())
                {
                    DbInitializer.Seed(context);
                }
            }

            app.Run();
        }
    }
}
