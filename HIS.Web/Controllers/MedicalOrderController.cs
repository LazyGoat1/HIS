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
    public class MedicalOrderController : Controller
    {
        private readonly IMedicalOrderService _orderService;
        private readonly IInpatientService _inpatientService;
        private readonly ISysLogService _logService;

        public MedicalOrderController(
            IMedicalOrderService orderService, IInpatientService inpatientService, ISysLogService logService)
        {
            _orderService = orderService;
            _inpatientService = inpatientService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, int? orderType = null, int? status = null,
            long? inpatientId = null, long? doctorId = null)
        {
            var query = new MedicalOrderQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, OrderType = orderType, Status = status,
                InpatientId = inpatientId, DoctorId = doctorId
            };
            var (list, total) = await _orderService.GetListAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        [HttpGet]
        public IActionResult Create(long inpatientId)
        {
            return View(new MedicalOrderDto
            {
                InpatientId = inpatientId,
                StartTime = DateTime.Now,
                OrderType = (int)MedicalOrderTypeEnum.Temporary
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] MedicalOrderDto dto)
        {
            if (dto.InpatientId <= 0) return ApiResult.Fail("住院ID无效");
            if (string.IsNullOrWhiteSpace(dto.OrderContent)) return ApiResult.Fail("医嘱内容不能为空");

            var userId = GetCurrentUserId();
            var (success, message) = await _orderService.CreateAsync(dto, userId);
            if (success) await LogAsync("住院管理", "下达医嘱", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Execute([FromBody] long id)
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _orderService.ExecuteAsync(id, userId);
            if (success) await LogAsync("住院管理", "执行医嘱", $"医嘱ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Complete([FromBody] long id)
        {
            var (success, message) = await _orderService.CompleteAsync(id);
            if (success) await LogAsync("住院管理", "完成医嘱", $"医嘱ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Stop([FromBody] long id)
        {
            var (success, message) = await _orderService.StopAsync(id);
            if (success) await LogAsync("住院管理", "停止医嘱", $"医嘱ID:{id}");
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
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
