using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class SysUserController : Controller
    {
        private readonly ISysUserService _userService;
        private readonly ISysRoleService _roleService;
        private readonly ISysDepartmentService _deptService;
        private readonly ISysLogService _logService;

        public SysUserController(ISysUserService userService, ISysRoleService roleService,
            ISysDepartmentService deptService, ISysLogService logService)
        {
            _userService = userService;
            _roleService = roleService;
            _deptService = deptService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10, string? keyword = null)
        {
            var (users, total) = await _userService.GetUserListAsync(pageIndex, pageSize, keyword);
            return LayuiTableResult.Ok(total, users);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View(new SysUserCreateDto { Status = 1 });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] SysUserCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _userService.CreateUserAsync(dto, userId);

            if (success)
                await LogAsync("用户管理", "新增用户", $"新增用户 {dto.UserName}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();

            var dto = new SysUserCreateDto
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                Gender = user.Gender,
                Phone = user.Phone,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                RoleId = user.RoleId,
                Status = user.Status
            };

            return View("Create", dto);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] SysUserCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _userService.UpdateUserAsync(dto, userId);

            if (success)
                await LogAsync("用户管理", "编辑用户", $"编辑用户 {dto.UserName}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var (success, message) = await _userService.DeleteUserAsync(id);
            if (success) await LogAsync("用户管理", "删除用户", $"删除用户ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> SetStatus([FromBody] object data)
        {
            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var id = json.RootElement.GetProperty("id").GetInt64();
            var status = json.RootElement.GetProperty("status").GetInt32();

            var (success, message) = await _userService.SetUserStatusAsync(id, status);
            if (success) await LogAsync("用户管理", "状态变更", $"用户ID:{id} 状态=>{status}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> ResetPassword([FromBody] long id)
        {
            var (success, message, _) = await _userService.ResetPasswordAsync(id);
            if (success) await LogAsync("用户管理", "重置密码", $"重置用户ID:{id}的密码");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        private long GetCurrentUserId()
        {
            var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return val != null ? long.Parse(val) : 0;
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
