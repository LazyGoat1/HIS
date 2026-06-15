using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 患者信息管理控制器
    /// </summary>
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly ISysLogService _logService;

        public PatientController(IPatientService patientService, ISysLogService logService)
        {
            _patientService = patientService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10, string? keyword = null)
        {
            var (patients, total) = await _patientService.GetPatientListAsync(pageIndex, pageSize, keyword);
            return LayuiTableResult.Ok(total, patients);
        }

        public IActionResult Create()
        {
            return View(new PatientDto());
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] PatientDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApiResult.Fail("患者姓名不能为空");

            var (success, message) = await _patientService.CreatePatientAsync(dto);
            if (success)
                await LogAsync("基础数据", "新增患者", $"新增患者 {dto.Name}({dto.PatientNo})");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound("患者不存在");

            return View("Create", patient);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] PatientDto dto)
        {
            var (success, message) = await _patientService.UpdatePatientAsync(dto);
            if (success)
                await LogAsync("基础数据", "编辑患者", $"编辑患者 {dto.Name}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<IActionResult> Export(string? ids = null)
        {
            var (allPatients, _) = await _patientService.GetPatientListAsync(1, 9999);
            var patients = allPatients;
            if (!string.IsNullOrEmpty(ids))
            { var idList = ids.Split(',').Select(long.Parse).ToHashSet(); patients = patients.Where(p => idList.Contains(p.Id)).ToList(); }
            var data = patients.Select(p => new PatientExportDto
            { PatientNo = p.PatientNo, Name = p.Name, Gender = p.Gender == 1 ? "男" : "女", Age = p.Age, IdCard = p.IdCard, Phone = p.Phone, Address = p.Address, BloodType = p.BloodType, AllergyHistory = p.AllergyHistory }).ToList();
            var bytes = HIS.Common.Helpers.ExcelHelper.Export(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "患者导出.xlsx");
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            var (success, message) = await _patientService.DeletePatientAsync(id);
            if (success)
                await LogAsync("基础数据", "删除患者", $"删除患者 {patient?.Name}(ID:{id})");

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
