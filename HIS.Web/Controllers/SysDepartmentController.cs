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
    public class SysDepartmentController : Controller
    {
        private readonly ISysDepartmentService _deptService;
        private readonly ISysLogService _logService;

        public SysDepartmentController(ISysDepartmentService deptService, ISysLogService logService)
        {
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
            var (depts, total) = await _deptService.GetDepartmentListAsync(pageIndex, pageSize, keyword);
            return LayuiTableResult.Ok(total, depts);
        }

        [HttpGet]
        public async Task<ApiResult> GetTree()
        {
            var tree = await _deptService.GetDepartmentTreeAsync();
            return ApiResult.Success(data: tree);
        }

        public IActionResult Create()
        {
            ViewBag.DeptTypes = new[]
            {
                new { Value = 1, Name = "临床科室" },
                new { Value = 2, Name = "医技科室" },
                new { Value = 3, Name = "行政科室" }
            };
            return View(new SysDepartmentDto { Status = 1 });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] SysDepartmentDto dto)
        {
            var (success, message) = await _deptService.CreateDepartmentAsync(dto);
            if (success) await LogAsync("科室管理", "新增科室", $"新增科室 {dto.DeptName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var dept = await _deptService.GetDepartmentByIdAsync(id);
            if (dept == null) return NotFound();
            ViewBag.DeptTypes = new[]
            {
                new { Value = 1, Name = "临床科室" },
                new { Value = 2, Name = "医技科室" },
                new { Value = 3, Name = "行政科室" }
            };
            return View("Create", dept);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] SysDepartmentDto dto)
        {
            var (success, message) = await _deptService.UpdateDepartmentAsync(dto);
            if (success) await LogAsync("科室管理", "编辑科室", $"编辑科室 {dto.DeptName}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var (success, message) = await _deptService.DeleteDepartmentAsync(id);
            if (success) await LogAsync("科室管理", "删除科室", $"删除科室ID:{id}");
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
