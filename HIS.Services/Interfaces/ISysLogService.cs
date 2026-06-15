namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 系统日志服务接口
    /// </summary>
    public interface ISysLogService
    {
        /// <summary>记录操作日志</summary>
        Task LogAsync(long? userId, string? userName, string module, string action,
            string? description, string? requestUrl, string? ipAddress, long elapsed = 0);

        /// <summary>获取日志列表</summary>
        Task<(List<object> Logs, int Total)> GetLogListAsync(int pageIndex, int pageSize,
            string? keyword = null, string? module = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
