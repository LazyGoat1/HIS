using HIS.Common.Enums;
using HIS.Models;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 挂号管理控制器
    /// </summary>
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _regService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;
        private readonly ISysDepartmentService _deptService;
        private readonly ISysLogService _logService;
        private readonly IBaseRepository<Schedule> _scheduleRepo;

        public RegistrationController(
            IRegistrationService regService,
            IPatientService patientService,
            IDoctorService doctorService,
            ISysDepartmentService deptService,
            ISysLogService logService,
            IBaseRepository<Schedule> scheduleRepo)
        {
            _regService = regService;
            _patientService = patientService;
            _doctorService = doctorService;
            _deptService = deptService;
            _logService = logService;
            _scheduleRepo = scheduleRepo;
        }

        public async Task<IActionResult> Index()
        {
            // 判断当前用户是否是医生，传给前端控制按钮显示
            var userId = GetCurrentUserId();
            var doctors = await _doctorService.GetAvailableDoctorsAsync();
            var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
            ViewBag.IsDoctor = doctor != null;
            ViewBag.DoctorId = doctor?.Id ?? 0;
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(
            int pageIndex = 1, int pageSize = 10, string? keyword = null,
            int? registrationType = null, int? status = null,
            string? visitDateStart = null, string? visitDateEnd = null,
            long? departmentId = null, long? doctorId = null)
        {
            var query = new RegistrationQueryModel
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Keyword = keyword,
                RegistrationType = registrationType,
                Status = status,
                DepartmentId = departmentId,
                DoctorId = doctorId
            };

            // 医生角色：自动只显示挂到自己名下的数据
            if (!doctorId.HasValue)
            {
                var userId = GetCurrentUserId();
                var doctors = await _doctorService.GetAvailableDoctorsAsync();
                var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null) query.DoctorId = doctor.Id;
            }

            if (DateTime.TryParse(visitDateStart, out var ds)) query.VisitDateStart = ds;
            if (DateTime.TryParse(visitDateEnd, out var de)) query.VisitDateEnd = de;

            var (list, total) = await _regService.GetListAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _deptService.GetAllEnabledAsync();
            ViewBag.Doctors = await _doctorService.GetAvailableDoctorsAsync();
            return View(new RegistrationDto { VisitDate = DateTime.Today });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] RegistrationDto dto)
        {
            if (dto.PatientId <= 0) return ApiResult.Fail("请选择患者");
            if (dto.DepartmentId <= 0) return ApiResult.Fail("请选择科室");
            if (dto.DoctorId <= 0) return ApiResult.Fail("请选择医生");

            var userId = GetCurrentUserId();
            var (success, message) = await _regService.CreateAsync(dto, userId);

            if (success)
                await LogAsync("门诊管理", "新增挂号", message);

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Refund([FromBody] long id)
        {
            var (success, message) = await _regService.RefundAsync(id);
            if (success)
                await LogAsync("门诊管理", "退号", $"退号 ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<ApiResult<List<object>>> SearchPatient(string keyword)
        {
            var (patients, _) = await _patientService.GetPatientListAsync(1, 20, keyword);
            var result = patients.Select(p => new
            {
                id = p.Id,
                text = $"{p.Name} - {p.PatientNo}",
                name = p.Name,
                patientNo = p.PatientNo
            }).ToList<object>();
            return ApiResult<List<object>>.Success(data: result);
        }

        /// <summary>
        /// 医生"我的待接诊队列"：今日该医生的全部待接诊挂号
        /// </summary>
        [HttpGet]
        public async Task<ApiResult> GetMyQueue()
        {
            var userId = GetCurrentUserId();
            var doctors = await _doctorService.GetAvailableDoctorsAsync();
            var doctor = doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
                return ApiResult.Fail("当前用户不是医生");

            var list = await _regService.GetTodayListAsync(doctorId: doctor.Id);
            return ApiResult.Success(data: list);
        }

        [HttpGet]
        public async Task<ApiResult<List<object>>> GetDoctorsByDepartment(long departmentId)
        {
            var doctors = await _doctorService.GetAvailableDoctorsAsync(departmentId);
            // 只返回今天有排班的医生
            var todayDayOfWeek = (int)DateTime.Now.DayOfWeek;
            var scheduledDoctorIds = _scheduleRepo.GetQueryable()
                .Where(s => s.DayOfWeek == todayDayOfWeek && s.Status == 1 && s.DepartmentId == departmentId)
                .Select(s => s.DoctorId).ToHashSet();
            if (scheduledDoctorIds.Any())
                doctors = doctors.Where(d => scheduledDoctorIds.Contains(d.Id)).ToList();

            var result = doctors.Select(d => (object)new
            {
                id = d.Id,
                name = d.Name,
                doctorNo = d.DoctorNo,
                title = ((DoctorTitleEnum)d.Title).ToString(),
                fee = d.ConsultationFee
            }).ToList();
            return ApiResult<List<object>>.Success(data: result);
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
