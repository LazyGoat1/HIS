using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HIS.Web.Filters
{
    /// <summary>
    /// 登录验证过滤器
    /// </summary>
    public class AuthFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 检查是否已认证
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                // 判断是否为 Ajax 请求
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new JsonResult(new { code = 401, msg = "未登录或登录已过期" });
                }
                else
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                }
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || request.ContentType?.Contains("application/json") == true;
        }
    }

    /// <summary>
    /// 权限验证过滤器
    /// </summary>
    public class PermissionFilter : IAuthorizationFilter
    {
        private readonly string _permissionCode;

        public PermissionFilter(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // 超级管理员跳过权限检查
            if (user.IsInRole("super_admin"))
                return;

            // 检查是否有指定权限
            var permissions = user.FindFirst("Permissions")?.Value ?? "";
            if (!permissions.Contains(_permissionCode))
            {
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new JsonResult(new { code = 403, msg = "没有操作权限" });
                }
                else
                {
                    context.Result = new ViewResult { ViewName = "AccessDenied" };
                }
            }
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
