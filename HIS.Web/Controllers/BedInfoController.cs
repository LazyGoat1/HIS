using HIS.Models;
using HIS.Models.DTOs;
using HIS.Models.QueryModels;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class BedInfoController : Controller
    {
        private readonly IBedService _bedService;
        private readonly ISysDepartmentService _deptService;
        private readonly ISysLogService _logService;

        public BedInfoController(IBedService bedService, ISysDepartmentService deptService, ISysLogService logService)
        {
            _bedService = bedService;
            _deptService = deptService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, long? departmentId = null, int? status = null, int? bedType = null)
        {
            var query = new BedQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, DepartmentId = departmentId,
                Status = status, BedType = bedType
            };
            var (list, total) = await _bedService.GetListAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View(new BedInfoDto());
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] BedInfoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BedNo)) return ApiResult.Fail("床位号不能为空");
            if (dto.DepartmentId <= 0) return ApiResult.Fail("请选择科室");
            var (success, message) = await _bedService.CreateAsync(dto);
            if (success) await LogAsync("住院管理", "新增床位", $"新增床位 {dto.BedNo}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var bed = await _bedService.GetByIdAsync(id);
            if (bed == null) return NotFound();
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View("Create", bed);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] BedInfoDto dto)
        {
            var (success, message) = await _bedService.UpdateAsync(dto);
            if (success) await LogAsync("住院管理", "编辑床位", $"编辑床位 {dto.BedNo}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var bed = await _bedService.GetByIdAsync(id);
            var (success, message) = await _bedService.DeleteAsync(id);
            if (success) await LogAsync("住院管理", "删除床位", $"删除床位 {bed?.BedNo}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        private async Task LogAsync(string m, string a, string? d)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _logService.LogAsync(uid != null ? long.Parse(uid) : null,
                User.Identity?.Name, m, a, d, HttpContext.Request.Path, ip);
        }
    }
}
