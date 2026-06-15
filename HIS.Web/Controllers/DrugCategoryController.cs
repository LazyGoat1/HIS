using HIS.Models;
using HIS.Models.DTOs;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class DrugCategoryController : Controller
    {
        private readonly IDrugCategoryService _service;
        private readonly ISysLogService _logService;

        public DrugCategoryController(IDrugCategoryService service, ISysLogService logService)
        {
            _service = service;
            _logService = logService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<ApiResult> GetTree()
        {
            var tree = await _service.GetTreeAsync();
            return ApiResult.Success(data: tree);
        }

        [HttpGet]
        public async Task<ApiResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return ApiResult.Success(data: list);
        }

        public IActionResult Create(long? parentId = null)
        {
            return View(new DrugCategoryDto { ParentId = parentId, SortOrder = 1 });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] DrugCategoryDto dto)
        {
            var (success, message) = await _service.CreateAsync(dto);
            if (success) await LogAsync("药品分类", "新增", dto.CategoryName);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var cat = await _service.GetByIdAsync(id);
            return cat == null ? NotFound() : View("Create", cat);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] DrugCategoryDto dto)
        {
            var (success, message) = await _service.UpdateAsync(dto);
            if (success) await LogAsync("药品分类", "编辑", dto.CategoryName);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        {
            var cat = await _service.GetByIdAsync(id);
            var (success, message) = await _service.DeleteAsync(id);
            if (success) await LogAsync("药品分类", "删除", cat?.CategoryName);
            return success ? ApiResult.Success(message) : ApiResult.Fail(message);
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
