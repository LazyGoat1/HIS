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
    /// <summary>
    /// 处方管理控制器
    /// </summary>
    [Authorize]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IOutpatientService _outpatientService;
        private readonly IDrugService _drugService;
        private readonly ISysLogService _logService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IOutpatientService outpatientService,
            IDrugService drugService,
            ISysLogService logService)
        {
            _prescriptionService = prescriptionService;
            _outpatientService = outpatientService;
            _drugService = drugService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(
            int pageIndex = 1, int pageSize = 10, string? keyword = null,
            int? prescriptionType = null, int? status = null,
            string? startDate = null, string? endDate = null,
            long? doctorId = null)
        {
            var query = new PrescriptionQueryModel
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Keyword = keyword,
                PrescriptionType = prescriptionType,
                Status = status,
                DoctorId = doctorId
            };

            if (DateTime.TryParse(startDate, out var sd)) query.StartDate = sd;
            if (DateTime.TryParse(endDate, out var ed)) query.EndDate = ed;

            var (list, total) = await _prescriptionService.GetListAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(long outpatientRecordId)
        {
            var record = await _outpatientService.GetByIdAsync(outpatientRecordId);
            if (record == null) return NotFound("门诊诊疗记录不存在");

            var (drugs, _) = await _drugService.GetDrugListAsync(1, 200);

            ViewBag.Drugs = drugs;
            ViewBag.OutpatientRecord = record;

            return View(new PrescriptionDto
            {
                OutpatientRecordId = outpatientRecordId,
                PatientId = record.PatientId,
                PatientName = record.PatientName
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] PrescriptionDto dto)
        {
            if (dto.OutpatientRecordId <= 0)
                return ApiResult.Fail("门诊记录ID无效");

            if (dto.Details == null || dto.Details.Count == 0)
                return ApiResult.Fail("请至少添加一个处方药品");

            var userId = GetCurrentUserId();
            var (success, message) = await _prescriptionService.CreateAsync(dto, userId);

            if (success)
                await LogAsync("门诊管理", "开具处方", message);

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<IActionResult> Print(long id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);
            if (prescription == null) return NotFound();
            return View(prescription);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id);
            if (prescription == null) return NotFound("处方不存在");

            return View(prescription);
        }

        [HttpGet]
        public async Task<ApiResult<List<object>>> SearchDrug(string keyword, long? categoryId = null)
        {
            var (drugs, _) = await _drugService.GetDrugListAsync(1, 50, keyword, categoryId);
            var result = drugs.Select(d => (object)new
            {
                id = d.Id, name = d.DrugName, code = d.DrugCode,
                category = d.CategoryName ?? "", specification = d.Specification,
                unit = d.Unit, retailPrice = d.RetailPrice, stockQuantity = d.StockQuantity
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
