using HIS.Common.Enums;
using HIS.Repository.Data;
using HIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly HisDbContext _context;

        public StatisticsService(HisDbContext context) { _context = context; }

        public async Task<object> GetDashboardSummaryAsync()
        {
            var today = DateTime.Today;
            return new
            {
                todayRegistrations = await _context.Registrations.CountAsync(r => r.VisitDate.Date == today),
                todayOutpatients = await _context.OutpatientRecords.CountAsync(o => o.VisitTime.Date == today),
                inpatientCount = await _context.InpatientRecords.CountAsync(i => i.Status == 1),
                todayIncome = await _context.ChargeRecords
                    .Where(c => c.CreateTime.Date == today && c.Status == 1)
                    .SumAsync(c => (decimal?)c.PaidAmount) ?? 0
            };
        }

        public async Task<List<object>> GetOutpatientTrendAsync(int days)
        {
            var start = DateTime.Today.AddDays(-days + 1);
            var data = await _context.OutpatientRecords
                .Where(o => o.VisitTime >= start)
                .GroupBy(o => o.VisitTime.Date)
                .Select(g => new { date = g.Key, count = g.Count() })
                .OrderBy(g => g.date).ToListAsync();

            return FillDateRange(start, DateTime.Today, data, d => new { date = d.date.ToString("MM-dd"), value = d.count });
        }

        public async Task<List<object>> GetIncomeTrendAsync(int days)
        {
            var start = DateTime.Today.AddDays(-days + 1);
            var data = await _context.ChargeRecords
                .Where(c => c.CreateTime >= start && c.Status == (int)ChargeStatusEnum.Charged)
                .GroupBy(c => c.CreateTime.Date)
                .Select(g => new { date = g.Key, total = g.Sum(c => c.PaidAmount) })
                .OrderBy(g => g.date).ToListAsync();

            return FillDateRange(start, DateTime.Today, data, d => new { date = d.date.ToString("MM-dd"), value = d.total });
        }

        public async Task<List<object>> GetDepartmentRankingAsync()
        {
            return (await _context.OutpatientRecords
                .Include(o => o.Registration).ThenInclude(r => r!.Department)
                .GroupBy(o => new { o.Registration!.DepartmentId, DeptName = o.Registration.Department!.DeptName })
                .Select(g => new { dept = g.Key.DeptName, count = g.Count() })
                .OrderByDescending(g => g.count).Take(10).ToListAsync())
                .Select(g => (object)new { name = g.dept, value = g.count }).ToList();
        }

        public async Task<List<object>> GetDrugRankingAsync(int topN)
        {
            return (await _context.PrescriptionDetails
                .Where(d => d.ItemType == (int)PrescriptionItemTypeEnum.Drug)
                .GroupBy(d => d.ItemName)
                .Select(g => new { drug = g.Key, qty = g.Sum(d => d.Quantity) })
                .OrderByDescending(g => g.qty).Take(topN).ToListAsync())
                .Select(g => (object)new { name = g.drug, value = g.qty }).ToList();
        }

        private List<object> FillDateRange<T>(DateTime start, DateTime end, List<T> data, Func<T, object> map)
        {
            var dict = data.ToDictionary(d => ((dynamic)d!).date, d => map(d));
            var result = new List<object>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var key = d;
                result.Add(dict.ContainsKey(key) ? dict[key] : new { date = d.ToString("MM-dd"), value = (object)0 });
            }
            return result;
        }

        public async Task<object> GetDailySettlementAsync(DateTime? date = null)
        {
            var d = date ?? DateTime.Today;
            var charges = await _context.ChargeRecords
                .Where(c => c.CreateTime.Date == d.Date && c.Status == 1)
                .ToListAsync();
            return new
            {
                date = d.ToString("yyyy-MM-dd"),
                totalCount = charges.Count,
                totalAmount = charges.Sum(c => c.PaidAmount),
                registrationCount = charges.Count(c => c.ChargeType == 1),
                registrationAmount = charges.Where(c => c.ChargeType == 1).Sum(c => c.PaidAmount),
                outpatientCount = charges.Count(c => c.ChargeType == 2),
                outpatientAmount = charges.Where(c => c.ChargeType == 2).Sum(c => c.PaidAmount),
                cashAmount = charges.Where(c => c.PaymentMethod == 1).Sum(c => c.PaidAmount),
                wechatAmount = charges.Where(c => c.PaymentMethod == 2).Sum(c => c.PaidAmount),
                alipayAmount = charges.Where(c => c.PaymentMethod == 3).Sum(c => c.PaidAmount),
            };
        }

        #region 存储过程调用

        /// <summary>日收入报表（SP: sp_GetDailyRevenueReport）</summary>
        public async Task<List<object>> GetSpDailyRevenueAsync(DateTime? date = null)
        {
            var param = new Microsoft.Data.SqlClient.SqlParameter("@ReportDate", date ?? DateTime.Today);
            var items = await _context.Database
                .SqlQueryRaw<SpRevenueItem>("EXEC sp_GetDailyRevenueReport @ReportDate", param)
                .ToListAsync();
            return items.Select(i => (object)new { i.ChargeType, i.TypeName, i.TotalCount, i.TotalAmount }).ToList();
        }

        /// <summary>医生工作量（SP: sp_GetDoctorWorkload）</summary>
        public async Task<List<object>> GetSpDoctorWorkloadAsync(DateTime start, DateTime? end = null)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@StartDate", start);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@EndDate", (object?)end ?? DBNull.Value);
            return (await _context.Database
                .SqlQueryRaw<SpDoctorWorkloadItem>(
                    "EXEC sp_GetDoctorWorkload @StartDate, @EndDate", p1, p2)
                .ToListAsync())
                .Select(i => (object)new { i.DoctorId, i.DoctorName, i.DoctorNo, i.DepartmentName, i.RegistrationCount, i.ConsultationCount, i.PrescriptionCount }).ToList();
        }

        /// <summary>库存预警-存储过程版（SP: sp_GetDrugStockAlert）</summary>
        public async Task<List<object>> GetSpDrugStockAlertAsync()
        {
            return (await _context.Database
                .SqlQueryRaw<SpDrugAlertItem>("EXEC sp_GetDrugStockAlert")
                .ToListAsync())
                .Select(i => (object)new { i.Id, i.DrugCode, i.DrugName, i.Specification, i.Unit, i.StockQuantity, i.MinStock, i.RetailPrice, i.CategoryName, i.AlertLevel }).ToList();
        }

        /// <summary>科室就诊统计（SP: sp_GetDepartmentVisitStats）</summary>
        public async Task<List<object>> GetSpDeptVisitStatsAsync(string? yearMonth = null)
        {
            var p = new Microsoft.Data.SqlClient.SqlParameter("@YearMonth", (object?)yearMonth ?? DBNull.Value);
            return (await _context.Database
                .SqlQueryRaw<SpDeptVisitItem>("EXEC sp_GetDepartmentVisitStats @YearMonth", p)
                .ToListAsync())
                .Select(i => (object)new { i.DeptId, i.DeptName, i.VisitCount, i.ConsultCount, i.TotalRevenue }).ToList();
        }

        /// <summary>处方发药汇总（SP: sp_GetPrescriptionDrugSummary）</summary>
        public async Task<List<object>> GetSpPrescriptionDrugSummaryAsync(DateTime start, DateTime? end = null)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@StartDate", start);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@EndDate", (object?)end ?? DBNull.Value);
            return (await _context.Database
                .SqlQueryRaw<SpDrugSummaryItem>(
                    "EXEC sp_GetPrescriptionDrugSummary @StartDate, @EndDate", p1, p2)
                .ToListAsync())
                .Select(i => (object)new { i.DrugName, i.Specification, i.Unit, i.TotalQuantity, i.TotalAmount, i.PrescriptionCount }).ToList();
        }

        // SP 返回模型（与存储过程输出列匹配）
        private class SpRevenueItem { public int ChargeType { get; set; } public string TypeName { get; set; } = ""; public int? TotalCount { get; set; } public decimal? TotalAmount { get; set; } }
        private class SpDoctorWorkloadItem { public long DoctorId { get; set; } public string DoctorName { get; set; } = ""; public string DoctorNo { get; set; } = ""; public string DepartmentName { get; set; } = ""; public int RegistrationCount { get; set; } public int ConsultationCount { get; set; } public int PrescriptionCount { get; set; } }
        private class SpDrugAlertItem { public long Id { get; set; } public string DrugCode { get; set; } = ""; public string DrugName { get; set; } = ""; public string? Specification { get; set; } public string? Unit { get; set; } public int StockQuantity { get; set; } public int MinStock { get; set; } public decimal RetailPrice { get; set; } public string? CategoryName { get; set; } public string AlertLevel { get; set; } = ""; }
        private class SpDeptVisitItem { public long DeptId { get; set; } public string DeptName { get; set; } = ""; public int VisitCount { get; set; } public int ConsultCount { get; set; } public decimal TotalRevenue { get; set; } }
        private class SpDrugSummaryItem { public string DrugName { get; set; } = ""; public string? Specification { get; set; } public string? Unit { get; set; } public int TotalQuantity { get; set; } public decimal TotalAmount { get; set; } public int PrescriptionCount { get; set; } }

        #endregion
    }
}
