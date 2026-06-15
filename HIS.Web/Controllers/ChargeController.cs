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
    public class ChargeController : Controller
    {
        private readonly IChargeService _chargeService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IPatientService _patientService;
        private readonly ISysLogService _logService;

        public ChargeController(IChargeService chargeService, IPrescriptionService prescriptionService,
            IPatientService patientService, ISysLogService logService)
        {
            _chargeService = chargeService;
            _prescriptionService = prescriptionService;
            _patientService = patientService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, int? chargeType = null, int? status = null,
            int? paymentMethod = null, string? startDate = null, string? endDate = null)
        {
            var q = new ChargeQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, ChargeType = chargeType,
                Status = status, PaymentMethod = paymentMethod
            };
            if (DateTime.TryParse(startDate, out var sd)) q.StartDate = sd;
            if (DateTime.TryParse(endDate, out var ed)) q.EndDate = ed;

            var (list, total) = await _chargeService.GetListAsync(q);
            return LayuiTableResult.Ok(total, list);
        }

        public IActionResult Create(long? relatedId = null, long? patientId = null,
            decimal? amount = null, string? patientName = null)
        {
            ViewBag.PatientName = patientName;
            return View(new ChargeCreateDto
            {
                RelatedId = relatedId,
                PatientId = patientId ?? 0,
                TotalAmount = amount ?? 0,
                PaidAmount = amount ?? 0,
                PaymentMethod = 1
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] ChargeCreateDto dto)
        {
            if (dto.PatientId <= 0) return ApiResult.Fail("请选择患者");
            if (dto.TotalAmount <= 0) return ApiResult.Fail("请输入应收金额");
            var userId = GetCurrentUserId();
            var (success, message) = await _chargeService.CreateAsync(dto, userId);
            if (success) await LogAsync("收费管理", "收费", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Refund([FromBody] long id)
        {
            var (success, message) = await _chargeService.RefundAsync(id);
            if (success) await LogAsync("收费管理", "退费", $"退费ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<ApiResult> SearchPrescription(string keyword)
        {
            var query = new PrescriptionQueryModel
            { PageIndex = 1, PageSize = 20, Keyword = keyword, Status = (int)PrescriptionStatusEnum.Issued };
            var (list, _) = await _prescriptionService.GetPendingDispenseAsync(query);
            var result = list.Select(p => new
            { id = p.Id, prescriptionNo = p.PrescriptionNo, patientName = p.PatientName, totalAmount = p.TotalAmount }).ToList();
            return ApiResult.Success(data: result);
        }

        [HttpGet]
        public async Task<IActionResult> Export(string? ids = null)
        {
            var q = new ChargeQueryModel { PageIndex = 1, PageSize = 9999 };
            var (allCharges, _) = await _chargeService.GetListAsync(q);
            var charges = allCharges;
            if (!string.IsNullOrEmpty(ids))
            {
                var idList = ids.Split(',').Select(long.Parse).ToHashSet();
                charges = charges.Where(c => idList.Contains(c.Id)).ToList();
            }
            var data = charges.Select(c => new ChargeExportDto
            {
                ChargeNo = c.ChargeNo, PatientName = c.PatientName,
                ChargeType = c.ChargeType == 1 ? "挂号" : c.ChargeType == 2 ? "门诊" : c.ChargeType == 3 ? "住院预交" : "住院结算",
                TotalAmount = c.TotalAmount, PaidAmount = c.PaidAmount,
                PaymentMethod = c.PaymentMethod == 1 ? "现金" : c.PaymentMethod == 2 ? "微信" : c.PaymentMethod == 3 ? "支付宝" : c.PaymentMethod == 4 ? "银行卡" : "医保",
                Status = c.Status == 1 ? "已收费" : "已退费",
                CreateTime = c.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();
            var bytes = HIS.Common.Helpers.ExcelHelper.Export(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "收费导出.xlsx");
        }

        [HttpGet]
        public async Task<ApiResult> SearchPatient(string keyword)
        {
            var (patients, _) = await _patientService.GetPatientListAsync(1, 10, keyword);
            var result = patients.Select(p => new { id = p.Id, name = p.Name, patientNo = p.PatientNo }).ToList();
            return ApiResult.Success(data: result);
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
