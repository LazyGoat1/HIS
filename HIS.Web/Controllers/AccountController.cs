using System.Security.Claims;
using HIS.Common.Extensions;
using HIS.Models;
using HIS.Models.ViewModels;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 账户（登录/登出/个人信息）
    /// 页面渲染方法保留 IActionResult，JSON API 方法直接返回 ApiResult
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ISysUserService _userService;
        private readonly ISysLogService _logService;

        public AccountController(ISysUserService userService, ISysLogService logService)
        {
            _userService = userService;
            _logService = logService;
        }

        #region 页面渲染

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            if (userId != null)
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                await _logService.LogAsync(userId.ToLong(), userName, "系统登出", "登出成功", null, "/Account/Logout", ip);
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        #endregion

        #region JSON API

        [HttpPost]
        public async Task<ApiResult> Login([FromBody] LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return ApiResult.Fail("请输入用户名和密码");

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var (success, message, user) = await _userService.LoginAsync(model.UserName, model.Password, ip);

            if (!success || user == null)
            {
                await _logService.LogAsync(null, model.UserName, "系统登录", "登录失败", message, "/Account/Login", ip);
                return ApiResult.Fail(message);
            }

            // 创建 Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new("RealName", user.RealName),
                new("RoleId", user.RoleId.ToString()),
                new("DepartmentId", user.DepartmentId?.ToString() ?? "0")
            };

            claims.Add(new Claim(ClaimTypes.Role, user.RoleId.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            await _logService.LogAsync(user.Id, user.UserName, "系统登录", "登录成功",
                $"用户 {user.UserName}({user.RealName}) 登录系统", "/Account/Login", ip);

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("RealName", user.RealName);

            return ApiResult.Success("登录成功", new { redirectUrl = returnUrl ?? "/Home/Index" });
        }

        [HttpPost]
        public async Task<ApiResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return ApiResult.Fail("请填写完整信息");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return ApiResult.Fail("未登录");

            var (success, message) = await _userService.ChangePasswordAsync(
                userId.ToLong(), model.OldPassword, model.NewPassword);

            if (success)
                await LogAsync("个人设置", "修改密码", "修改登录密码");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        private async Task LogAsync(string m, string a, string? d)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _logService.LogAsync(uid != null ? long.Parse(uid) : null,
                User.Identity?.Name, m, a, d, HttpContext.Request.Path, ip);
        }

        /// <summary>
        /// 获取当前用户的侧边栏菜单
        /// </summary>
        [HttpGet]
        public async Task<ApiResult> GetMenus()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return ApiResult.Fail("未登录");

            var menus = await _userService.GetUserMenusAsync(userId.ToLong());
            return ApiResult.Success(data: menus);
        }

        /// <summary>
        /// 获取当前登录用户完整信息
        /// </summary>
        [HttpGet]
        public async Task<ApiResult> GetUserInfo()
        {
            if (User.Identity?.IsAuthenticated != true)
                return ApiResult.Fail("未登录");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ApiResult.Fail("未登录");
            var user = await _userService.GetUserByIdAsync(userId.ToLong());

            return ApiResult.Success(data: new
            {
                userName = user?.UserName,
                realName = user?.RealName,
                phone = user?.Phone,
                email = user?.Email,
                avatar = user?.Avatar,
                roleId = User.FindFirst("RoleId")?.Value
            });
        }

        /// <summary>
        /// 保存个人信息
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> Profile([FromBody] object data)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return ApiResult.Fail("未登录");

            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var realName = json.RootElement.TryGetProperty("RealName", out var rn) ? rn.GetString() : null;
            var phone = json.RootElement.TryGetProperty("Phone", out var p) ? p.GetString() : null;
            var email = json.RootElement.TryGetProperty("Email", out var e) ? e.GetString() : null;
            var avatar = json.RootElement.TryGetProperty("Avatar", out var a) ? a.GetString() : null;

            var (success, message) = await _userService.UpdateProfileAsync(userId.ToLong(), realName, phone, email, avatar);
            if (success) await LogAsync("个人设置", "修改信息", $"修改个人信息");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0) return ApiResult.Fail("请选择图片");
            if (file.Length > 1024 * 1024) return ApiResult.Fail("图片不能超过1MB");

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var url = $"/uploads/avatars/{fileName}";
            return ApiResult.Success(data: url);
        }

        #endregion
    }
}
