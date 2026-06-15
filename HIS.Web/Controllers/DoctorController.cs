using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 医生信息管理控制器
    /// </summary>
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly ISysDepartmentService _deptService;
        private readonly ISysUserService _userService;
        private readonly ISysLogService _logService;

        public DoctorController(
            IDoctorService doctorService,
            ISysDepartmentService deptService,
            ISysUserService userService,
            ISysLogService logService)
        {
            _doctorService = doctorService;
            _deptService = deptService;
            _userService = userService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, long? departmentId = null)
        {
            var (doctors, total) = await _doctorService.GetDoctorListAsync(
                pageIndex, pageSize, keyword, departmentId);
            return LayuiTableResult.Ok(total, doctors);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View(new DoctorDto
            {
                Status = 1,
                MaxDailyPatients = 50,
                ConsultationFee = 15.00m
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] DoctorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResult.Fail("医生姓名不能为空");

            if (dto.DepartmentId <= 0)
                return ApiResult.Fail("请选择所属科室");

            var (success, message) = await _doctorService.CreateDoctorAsync(dto);
            if (success)
                await LogAsync("基础数据", "新增医生", $"新增医生 {dto.Name}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null) return NotFound("医生不存在");

            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View("Create", doctor);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] DoctorDto dto)
        {
            var (success, message) = await _doctorService.UpdateDoctorAsync(dto);
            if (success)
                await LogAsync("基础数据", "编辑医生", $"编辑医生 {dto.Name}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            var (success, message) = await _doctorService.DeleteDoctorAsync(id);
            if (success)
                await LogAsync("基础数据", "删除医生", $"删除医生 {doctor?.Name}(ID:{id})");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> ToggleStatus([FromBody] long id)
        {
            var (success, message) = await _doctorService.ToggleStatusAsync(id);
            if (success)
                await LogAsync("基础数据", "状态变更", $"切换医生ID:{id}状态");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        private async Task LogAsync(string module, string action, string? desc)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _logService.LogAsync(
                userId != null ? long.Parse(userId) : null,
                User.Identity?.Name, module, action, desc,
                HttpContext.Request.Path, ip);
        }
    }
}
