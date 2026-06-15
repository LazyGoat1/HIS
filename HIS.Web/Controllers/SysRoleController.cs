using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class SysRoleController : Controller
    {
        private readonly ISysRoleService _roleService;
        private readonly ISysMenuService _menuService;
        private readonly ISysLogService _logService;

        public SysRoleController(ISysRoleService roleService, ISysMenuService menuService, ISysLogService logService)
        {
            _roleService = roleService;
            _menuService = menuService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10, string? keyword = null)
        {
            var (roles, total) = await _roleService.GetRoleListAsync(pageIndex, pageSize, keyword);
            return LayuiTableResult.Ok(total, roles);
        }

        public IActionResult Create()
        {
            return View(new SysRoleDto { Status = 1 });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] SysRoleDto dto)
        {
            var (success, message) = await _roleService.CreateRoleAsync(dto);
            if (success) await LogAsync("角色管理", "新增角色", $"新增角色 {dto.RoleName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return role == null ? NotFound() : View("Create", role);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] SysRoleDto dto)
        {
            var (success, message) = await _roleService.UpdateRoleAsync(dto);
            if (success) await LogAsync("角色管理", "编辑角色", $"编辑角色 {dto.RoleName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var (success, message) = await _roleService.DeleteRoleAsync(id);
            if (success) await LogAsync("角色管理", "删除角色", $"删除角色ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        /// <summary>权限分配页面</summary>
        public async Task<IActionResult> Permission(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();

            ViewBag.RoleId = id;
            ViewBag.RoleName = role.RoleName;
            var menuTree = await _menuService.GetMenuTreeAsync();
            var checkedIds = await _roleService.GetRoleMenuIdsAsync(id);

            var jsonOpts = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            ViewBag.MenuTree = System.Text.Json.JsonSerializer.Serialize(menuTree, jsonOpts);
            ViewBag.CheckedIds = System.Text.Json.JsonSerializer.Serialize(checkedIds, jsonOpts);

            return View();
        }

        [HttpPost]
        public async Task<ApiResult> SavePermission([FromBody] object data)
        {
            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var roleId = json.RootElement.GetProperty("roleId").GetInt64();
            var menuIds = json.RootElement.GetProperty("menuIds").EnumerateArray()
                .Select(e => e.GetInt64()).ToList();

            var (success, message) = await _roleService.SetRoleMenusAsync(roleId, menuIds);
            if (success) await LogAsync("角色管理", "权限分配", $"为角色ID:{roleId}分配权限");
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
