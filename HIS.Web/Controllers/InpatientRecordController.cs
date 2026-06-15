using HIS.Common.Enums;
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
    public class InpatientRecordController : Controller
    {
        private readonly IInpatientService _inpatientService;
        private readonly IPatientService _patientService;
        private readonly IBedService _bedService;
        private readonly ISysDepartmentService _deptService;
        private readonly ISysLogService _logService;

        public InpatientRecordController(
            IInpatientService inpatientService, IPatientService patientService,
            IBedService bedService, ISysDepartmentService deptService, ISysLogService logService)
        {
            _inpatientService = inpatientService;
            _patientService = patientService;
            _bedService = bedService;
            _deptService = deptService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, int? status = null, long? departmentId = null,
            string? admissionStart = null, string? admissionEnd = null)
        {
            var query = new InpatientQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, Status = status, DepartmentId = departmentId
            };
            if (DateTime.TryParse(admissionStart, out var ds)) query.AdmissionStart = ds;
            if (DateTime.TryParse(admissionEnd, out var de)) query.AdmissionEnd = de;

            var (list, total) = await _inpatientService.GetListAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            return View(new InpatientRecordDto { AdmissionTime = DateTime.Now });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] InpatientRecordDto dto)
        {
            if (dto.PatientId <= 0) return ApiResult.Fail("请选择患者");
            if (dto.DepartmentId <= 0) return ApiResult.Fail("请选择科室");

            var userId = GetCurrentUserId();
            var (success, message) = await _inpatientService.AdmitAsync(dto, userId);
            if (success) await LogAsync("住院管理", "入院登记", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Discharge([FromBody] object data)
        {
            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var id = json.RootElement.GetProperty("id").GetInt64();
            var diagnosis = json.RootElement.GetProperty("diagnosis").GetString() ?? "";

            var (success, message) = await _inpatientService.DischargeAsync(id, diagnosis);
            if (success) await LogAsync("住院管理", "出院结算", $"住院ID:{id} 出院");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<ApiResult> GetAvailableBeds(long departmentId)
        {
            var beds = await _bedService.GetAvailableBedsAsync(departmentId);
            return ApiResult.Success(data: beds);
        }

        [HttpGet]
        public async Task<ApiResult<List<object>>> SearchPatient(string keyword)
        {
            var (patients, _) = await _patientService.GetPatientListAsync(1, 20, keyword);
            var result = patients.Select(p => (object)new
            { id = p.Id, text = $"{p.Name} - {p.PatientNo}", name = p.Name, patientNo = p.PatientNo }).ToList();
            return ApiResult<List<object>>.Success(data: result);
        }

        private long GetCurrentUserId()
        {
            var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return val != null ? long.Parse(val) : 0;
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
