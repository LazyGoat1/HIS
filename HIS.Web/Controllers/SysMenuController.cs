using HIS.Common.Enums;
using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class SysMenuController : Controller
    {
        private readonly ISysMenuService _menuService;
        private readonly ISysLogService _logService;

        public SysMenuController(ISysMenuService menuService, ISysLogService logService)
        {
            _menuService = menuService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ApiResult> GetTree()
        {
            var tree = await _menuService.GetMenuTreeAsync();
            return ApiResult.Success(data: tree);
        }

        public IActionResult Create(long? parentId = null)
        {
            ViewBag.ParentId = parentId;
            ViewBag.MenuTypes = new[]
            {
                new { Value = 1, Name = "目录" },
                new { Value = 2, Name = "菜单" },
                new { Value = 3, Name = "按钮" }
            };
            return View(new SysMenuDto { ParentId = parentId, Status = 1, MenuType = 2 });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] SysMenuDto dto)
        {
            var (success, message) = await _menuService.CreateMenuAsync(dto);
            if (success) await LogAsync("菜单管理", "新增菜单", $"新增菜单 {dto.MenuName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null) return NotFound();
            ViewBag.MenuTypes = new[]
            {
                new { Value = 1, Name = "目录" },
                new { Value = 2, Name = "菜单" },
                new { Value = 3, Name = "按钮" }
            };
            return View("Create", menu);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] SysMenuDto dto)
        {
            var (success, message) = await _menuService.UpdateMenuAsync(dto);
            if (success) await LogAsync("菜单管理", "编辑菜单", $"编辑菜单 {dto.MenuName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var (success, message) = await _menuService.DeleteMenuAsync(id);
            if (success) await LogAsync("菜单管理", "删除菜单", $"删除菜单ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        private async Task LogAsync(string module, string action, string? desc)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _logService.LogAsync(
                userId != null ? long.Parse(userId) : null,
                User.Identity?.Name, module, action, desc, HttpContext.Request.Path, ip);
        }
    }
}
