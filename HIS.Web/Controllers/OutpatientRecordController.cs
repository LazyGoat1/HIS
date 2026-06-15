using HIS.Models;
using HIS.Models.DTOs;
using HIS.Models.QueryModels;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 门诊诊疗控制器
    /// </summary>
    [Authorize]
    public class OutpatientRecordController : Controller
    {
        private readonly IOutpatientService _outpatientService;
        private readonly IRegistrationService _regService;
        private readonly ISysLogService _logService;

        public OutpatientRecordController(
            IOutpatientService outpatientService,
            IRegistrationService regService,
            ISysLogService logService)
        {
            _outpatientService = outpatientService;
            _regService = regService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(
            int pageIndex = 1, int pageSize = 10, string? keyword = null,
            string? visitDateStart = null, string? visitDateEnd = null,
            long? doctorId = null)
        {
            var query = new OutpatientQueryModel
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Keyword = keyword,
                DoctorId = doctorId
            };

            if (DateTime.TryParse(visitDateStart, out var ds)) query.VisitDateStart = ds;
            if (DateTime.TryParse(visitDateEnd, out var de)) query.VisitDateEnd = de;

            var (records, total) = await _outpatientService.GetListAsync(query);
            return LayuiTableResult.Ok(total, records);
        }

        [HttpGet]
        public async Task<IActionResult> Create(long registrationId)
        {
            var reg = await _regService.GetByIdAsync(registrationId);
            if (reg == null) return NotFound("挂号记录不存在");

            var existing = await _outpatientService.GetByRegistrationIdAsync(registrationId);
            if (existing != null)
                return RedirectToAction("Detail", new { id = existing.Id });

            return View(new OutpatientRecordCreateDto { RegistrationId = registrationId });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] OutpatientRecordCreateDto dto)
        {
            if (dto.RegistrationId <= 0)
                return ApiResult.Fail("挂号ID无效");

            var userId = GetCurrentUserId();
            var (success, message) = await _outpatientService.CreateAsync(dto, userId);

            if (success)
            {
                // 接诊成功后，更新挂号状态为"已接诊"
                var doctors = await _regService.GetTodayListAsync();
                var reg = await _regService.GetByIdAsync(dto.RegistrationId);
                if (reg != null)
                    await _regService.AcceptAsync(dto.RegistrationId, reg.DoctorId);

                await LogAsync("门诊管理", "接诊", $"创建诊疗记录 挂号ID:{dto.RegistrationId}");
            }

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var record = await _outpatientService.GetByIdAsync(id);
            if (record == null) return NotFound("诊疗记录不存在");

            return View(record);
        }

        private long GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdStr != null ? long.Parse(userIdStr) : 0;
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
