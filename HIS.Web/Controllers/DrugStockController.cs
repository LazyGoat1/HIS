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
    public class DrugStockController : Controller
    {
        private readonly IDrugStockService _stockService;
        private readonly IDrugService _drugService;
        private readonly ISysLogService _logService;

        public DrugStockController(IDrugStockService stockService, IDrugService drugService, ISysLogService logService)
        {
            _stockService = stockService;
            _drugService = drugService;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, int? changeType = null, long? drugId = null,
            string? startDate = null, string? endDate = null)
        {
            var q = new DrugStockQueryModel
            {
                PageIndex = pageIndex, PageSize = pageSize,
                Keyword = keyword, ChangeType = changeType, DrugId = drugId
            };
            if (DateTime.TryParse(startDate, out var sd)) q.StartDate = sd;
            if (DateTime.TryParse(endDate, out var ed)) q.EndDate = ed;

            var (list, total) = await _stockService.GetListAsync(q);
            return LayuiTableResult.Ok(total, list);
        }

        public IActionResult StockIn() => View(new StockInDto());

        [HttpPost]
        public async Task<ApiResult> StockIn([FromBody] StockInDto dto)
        {
            if (dto.DrugId <= 0) return ApiResult.Fail("请选择药品");
            if (dto.Quantity <= 0) return ApiResult.Fail("入库数量必须大于0");
            var userId = GetCurrentUserId();
            var (success, message) = await _stockService.StockInAsync(dto, userId);
            if (success) await LogAsync("药房管理", "药品入库", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public IActionResult StockOut() => View(new StockOutDto());

        [HttpPost]
        public async Task<ApiResult> StockOut([FromBody] StockOutDto dto)
        {
            if (dto.DrugId <= 0) return ApiResult.Fail("请选择药品");
            if (dto.Quantity <= 0) return ApiResult.Fail("出库数量必须大于0");
            var userId = GetCurrentUserId();
            var (success, message) = await _stockService.StockOutAsync(dto, userId);
            if (success) await LogAsync("药房管理", "药品出库", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public IActionResult StockCheck() => View(new StockInDto());

        [HttpPost]
        public async Task<ApiResult> StockCheck([FromBody] object data)
        {
            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var drugId = json.RootElement.GetProperty("drugId").GetInt64();
            var qty = json.RootElement.GetProperty("quantity").GetInt32();
            var userId = GetCurrentUserId();
            var (success, message) = await _stockService.StockCheckAsync(drugId, qty, userId);
            if (success) await LogAsync("药房管理", "库存盘点", message);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<ApiResult> SearchDrug(string keyword)
        {
            var (drugs, _) = await _drugService.GetDrugListAsync(1, 20, keyword);
            var result = drugs.Select(d => (object)new
            { id = d.Id, name = d.DrugName, code = d.DrugCode, stock = d.StockQuantity, unit = d.Unit }).ToList();
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
