using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class ChargeItemController : Controller
    {
        private readonly IChargeItemService _service;

        public ChargeItemController(IChargeItemService service) => _service = service;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10, string? keyword = null)
        {
            var (list, total) = await _service.GetPagedAsync(pageIndex, pageSize, keyword);
            return LayuiTableResult.Ok(total, list);
        }

        public IActionResult Create() => View("Create", new ChargeItemDto());

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] ChargeItemDto dto)
        {
            var (ok, msg) = await _service.CreateAsync(dto);
            return ok ? ApiResult.Success(msg) : ApiResult.Fail(msg);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : View("Create", item);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] ChargeItemDto dto)
        {
            var (ok, msg) = await _service.UpdateAsync(dto);
            return ok ? ApiResult.Success(msg) : ApiResult.Fail(msg);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var (ok, msg) = await _service.DeleteAsync(id);
            return ok ? ApiResult.Success(msg) : ApiResult.Fail(msg);
        }

        /// <summary>供处方开具时搜索检查项目</summary>
        [HttpGet]
        public async Task<ApiResult> SearchExam(string? keyword)
        {
            var items = await _service.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(keyword))
                items = items.Where(i => i.ItemName.Contains(keyword) || i.Category.Contains(keyword)).ToList();
            var result = items.Select(i => new { i.Id, i.Category, i.ItemName, i.UnitPrice, i.Unit, i.Specification }).ToList();
            return ApiResult.Success(data: result);
        }
    }
}
