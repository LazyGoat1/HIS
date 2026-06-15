using HIS.Models;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statService;

        public StatisticsController(IStatisticsService statService) { _statService = statService; }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<ApiResult> Summary()
        {
            var data = await _statService.GetDashboardSummaryAsync();
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> OutpatientTrend(int days = 7)
        {
            var data = await _statService.GetOutpatientTrendAsync(days);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> IncomeTrend(int days = 7)
        {
            var data = await _statService.GetIncomeTrendAsync(days);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> DepartmentRanking()
        {
            var data = await _statService.GetDepartmentRankingAsync();
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> DailySettlement(string? date = null)
        {
            DateTime? dt = null;
            if (DateTime.TryParse(date, out var parsed)) dt = parsed;
            var data = await _statService.GetDailySettlementAsync(dt);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> DrugRanking(int topN = 10)
        {
            var data = await _statService.GetDrugRankingAsync(topN);
            return ApiResult.Success(data: data);
        }

        // ====== 存储过程端点 ======

        [HttpGet]
        public async Task<ApiResult> SpRevenue(string? date = null)
        {
            DateTime? dt = null;
            if (DateTime.TryParse(date, out var d)) dt = d;
            var data = await _statService.GetSpDailyRevenueAsync(dt);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> SpDoctorWorkload(string startDate, string? endDate = null)
        {
            if (!DateTime.TryParse(startDate, out var sd)) return ApiResult.Fail("日期格式错误");
            DateTime? ed = null;
            if (DateTime.TryParse(endDate, out var e)) ed = e;
            var data = await _statService.GetSpDoctorWorkloadAsync(sd, ed);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> SpDrugAlert()
        {
            var data = await _statService.GetSpDrugStockAlertAsync();
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> SpDeptVisit(string? yearMonth = null)
        {
            var data = await _statService.GetSpDeptVisitStatsAsync(yearMonth);
            return ApiResult.Success(data: data);
        }

        [HttpGet]
        public async Task<ApiResult> SpDrugSummary(string startDate, string? endDate = null)
        {
            if (!DateTime.TryParse(startDate, out var sd)) return ApiResult.Fail("日期格式错误");
            DateTime? ed = null;
            if (DateTime.TryParse(endDate, out var e)) ed = e;
            var data = await _statService.GetSpPrescriptionDrugSummaryAsync(sd, ed);
            return ApiResult.Success(data: data);
        }
    }
}
