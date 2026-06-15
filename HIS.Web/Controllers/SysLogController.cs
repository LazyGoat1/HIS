using HIS.Models;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class SysLogController : Controller
    {
        private readonly ISysLogService _logService;

        public SysLogController(ISysLogService logService)
        {
            _logService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, string? module = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var (logs, total) = await _logService.GetLogListAsync(pageIndex, pageSize, keyword, module, startDate, endDate);
            return LayuiTableResult.Ok(total, logs);
        }
    }
}
