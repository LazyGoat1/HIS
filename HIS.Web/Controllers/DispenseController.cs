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
    /// 处方发药控制器（药房角色使用）
    /// </summary>
    [Authorize]
    public class DispenseController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ISysLogService _logService;

        public DispenseController(IPrescriptionService prescriptionService, ISysLogService logService)
        {
            _prescriptionService = prescriptionService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, int? prescriptionType = null)
        {
            var query = new PrescriptionQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, PrescriptionType = prescriptionType
            };
            var (list, total) = await _prescriptionService.GetPendingDispenseAsync(query);
            return LayuiTableResult.Ok(total, list);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(long id)
        {
            var p = await _prescriptionService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        public async Task<ApiResult> Return([FromBody] long id)
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _prescriptionService.ReturnAsync(id, userId);
            if (success) await LogAsync("药房管理", "处方退药", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Dispense([FromBody] long id)
        {
            var userId = GetCurrentUserId();
            var (success, message) = await _prescriptionService.DispenseAsync(id, userId);
            if (success) await LogAsync("药房管理", "处方发药", message);
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
