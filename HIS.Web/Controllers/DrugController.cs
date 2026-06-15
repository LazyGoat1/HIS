using HIS.Models;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 药品字典管理控制器
    /// </summary>
    [Authorize]
    public class DrugController : Controller
    {
        private readonly IDrugService _drugService;
        private readonly IDrugCategoryService _categoryService;
        private readonly ISysLogService _logService;

        public DrugController(
            IDrugService drugService,
            IDrugCategoryService categoryService,
            ISysLogService logService)
        {
            _drugService = drugService;
            _categoryService = categoryService;
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, long? categoryId = null)
        {
            var (drugs, total) = await _drugService.GetDrugListAsync(pageIndex, pageSize, keyword, categoryId);
            return LayuiTableResult.Ok(total, drugs);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetHierarchicalAsync();
            return View(new DrugDto
            {
                Status = 1,
                MinStock = 10,
                IsPrescription = false,
                Unit = "盒"
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] DrugDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DrugName))
                return ApiResult.Fail("药品名称不能为空");

            var (success, message) = await _drugService.CreateDrugAsync(dto);
            if (success)
                await LogAsync("药品字典", "新增药品", $"新增药品 {dto.DrugName}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var drug = await _drugService.GetDrugByIdAsync(id);
            if (drug == null) return NotFound("药品不存在");

            ViewBag.Categories = await _categoryService.GetHierarchicalAsync();
            return View("Create", drug);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] DrugDto dto)
        {
            var (success, message) = await _drugService.UpdateDrugAsync(dto);
            if (success)
                await LogAsync("药品字典", "编辑药品", $"编辑药品 {dto.DrugName}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var drug = await _drugService.GetDrugByIdAsync(id);
            var (success, message) = await _drugService.DeleteDrugAsync(id);
            if (success)
                await LogAsync("药品字典", "删除药品", $"删除药品 {drug?.DrugName}(ID:{id})");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> SetStatus([FromBody] object data)
        {
            var json = System.Text.Json.JsonDocument.Parse(data.ToString()!);
            var id = json.RootElement.GetProperty("id").GetInt64();
            var status = json.RootElement.GetProperty("status").GetInt32();

            var (success, message) = await _drugService.SetStatusAsync(id, status);
            if (success)
                await LogAsync("药品字典", "状态变更", $"药品ID:{id} 状态=>{status}");

            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpGet]
        public async Task<IActionResult> Export(string? ids = null)
        {
            var (allDrugs, _) = await _drugService.GetDrugListAsync(1, 9999);
            var drugs = allDrugs;
            if (!string.IsNullOrEmpty(ids))
            {
                var idList = ids.Split(',').Select(long.Parse).ToHashSet();
                drugs = drugs.Where(d => idList.Contains(d.Id)).ToList();
            }
            var data = drugs.Select(d => new DrugExportDto
            {
                DrugCode = d.DrugCode, DrugName = d.DrugName, GenericName = d.GenericName,
                CategoryName = d.CategoryName, Specification = d.Specification, Unit = d.Unit,
                Manufacturer = d.Manufacturer, RetailPrice = d.RetailPrice,
                StockQuantity = d.StockQuantity, MinStock = d.MinStock
            }).ToList();
            var bytes = HIS.Common.Helpers.ExcelHelper.Export(data);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "药品导出.xlsx");
        }

        [HttpGet]
        public async Task<ApiResult> GetLowStock()
        {
            var list = await _drugService.GetLowStockDrugsAsync();
            return ApiResult.Success(data: list);
        }

        [HttpGet]
        public async Task<ApiResult> GetCategories()
        {
            var categories = await _categoryService.GetHierarchicalAsync();
            return ApiResult.Success(data: categories);
        }

        /// <summary>下载药品导入模板</summary>
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var bytes = HIS.Common.Helpers.ExcelHelper.GenerateTemplate<DrugImportDto>();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "药品导入模板.xlsx");
        }

        /// <summary>Excel 批量导入药品</summary>
        [HttpPost]
        public async Task<ApiResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return ApiResult.Fail("请选择文件");
            using var stream = file.OpenReadStream();
            var (data, errors) = HIS.Common.Helpers.ExcelHelper.Parse<DrugImportDto>(stream);
            if (!data.Any()) return ApiResult.Fail(errors.Any() ? string.Join("; ", errors.Take(5)) : "无有效数据");

            int success = 0, fail = 0;
            var allCategories = await _categoryService.GetAllAsync();
            foreach (var dto in data)
            {
                long? categoryId = null;
                if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                {
                    var cat = allCategories.FirstOrDefault(c => c.CategoryName.Contains(dto.CategoryName));
                    if (cat != null) categoryId = cat.Id;
                }
                var drugDto = new DrugDto
                {
                    DrugName = dto.DrugName, GenericName = dto.GenericName,
                    Specification = dto.Specification, Unit = dto.Unit ?? "盒",
                    Manufacturer = dto.Manufacturer, UnitPrice = dto.UnitPrice,
                    RetailPrice = dto.RetailPrice, StockQuantity = dto.StockQuantity,
                    MinStock = dto.MinStock, IsPrescription = dto.IsPrescriptionStr?.ToLower() == "true",
                    CategoryId = categoryId, Status = 1
                };
                var (ok, _) = await _drugService.CreateDrugAsync(drugDto);
                if (ok) success++; else fail++;
            }
            var msg = $"导入完成：成功 {success} 条";
            if (fail > 0) msg += $"，失败 {fail} 条";
            if (errors.Any()) msg += $"（格式错误: {string.Join("; ", errors.Take(3))}）";
            await LogAsync("药品字典", "批量导入", msg);
            return ApiResult.Success(msg);
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
