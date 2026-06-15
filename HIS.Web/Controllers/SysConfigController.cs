using HIS.Models;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class SysConfigController : Controller
    {
        private readonly IBaseRepository<SysConfig> _repo;
        private readonly IUnitOfWork _uow;

        public SysConfigController(IBaseRepository<SysConfig> repo, IUnitOfWork uow)
        { _repo = repo; _uow = uow; }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10, string? keyword = null)
        {
            var q = _repo.GetQueryable();
            if (!string.IsNullOrEmpty(keyword)) q = q.Where(c => c.ConfigKey.Contains(keyword) || (c.Description != null && c.Description.Contains(keyword)));
            var total = await Task.Run(() => q.Count());
            var list = await Task.Run(() => q.OrderBy(c => c.Id).Skip((pageIndex-1)*pageSize).Take(pageSize).ToList());
            return LayuiTableResult.Ok(total, list);
        }

        public IActionResult Create() => View("Create", new SysConfig());

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] SysConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ConfigKey)) return ApiResult.Fail("配置键不能为空");
            config.CreateTime = DateTime.Now;
            await _repo.AddAsync(config);
            await _uow.SaveChangesAsync();
            return ApiResult.Success("添加成功");
        }

        public async Task<IActionResult> Edit(long id)
        { var c = await _repo.GetByIdAsync(id); return c == null ? NotFound() : View("Create", c); }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] SysConfig config)
        {
            var existing = await _repo.GetByIdAsync(config.Id);
            if (existing == null) return ApiResult.Fail("不存在");
            existing.ConfigKey = config.ConfigKey; existing.ConfigValue = config.ConfigValue; existing.Description = config.Description;
            _repo.Update(existing); await _uow.SaveChangesAsync();
            return ApiResult.Success("更新成功");
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        { var c = await _repo.GetByIdAsync(id); if (c == null) return ApiResult.Fail("不存在"); _repo.Delete(c); await _uow.SaveChangesAsync(); return ApiResult.Success("已删除"); }
    }
}
