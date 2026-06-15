namespace HIS.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<object> GetDashboardSummaryAsync();
        Task<List<object>> GetOutpatientTrendAsync(int days);
        Task<List<object>> GetIncomeTrendAsync(int days);
        Task<List<object>> GetDepartmentRankingAsync();
        Task<List<object>> GetDrugRankingAsync(int topN);
        Task<object> GetDailySettlementAsync(DateTime? date = null);

        // 存储过程
        Task<List<object>> GetSpDailyRevenueAsync(DateTime? date = null);
        Task<List<object>> GetSpDoctorWorkloadAsync(DateTime start, DateTime? end = null);
        Task<List<object>> GetSpDrugStockAlertAsync();
        Task<List<object>> GetSpDeptVisitStatsAsync(string? yearMonth = null);
        Task<List<object>> GetSpPrescriptionDrugSummaryAsync(DateTime start, DateTime? end = null);
    }
}
