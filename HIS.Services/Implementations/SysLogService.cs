using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 系统日志服务实现
    /// </summary>
    public class SysLogService : ISysLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<SysLog> _logRepository;

        public SysLogService(IUnitOfWork unitOfWork, IBaseRepository<SysLog> logRepository)
        {
            _unitOfWork = unitOfWork;
            _logRepository = logRepository;
        }

        public async Task LogAsync(long? userId, string? userName, string module, string action,
            string? description, string? requestUrl, string? ipAddress, long elapsed = 0)
        {
            var log = new SysLog
            {
                UserId = userId,
                UserName = userName,
                Module = module,
                Action = action,
                Description = description,
                RequestUrl = requestUrl,
                IPAddress = ipAddress,
                Elapsed = elapsed,
                CreateTime = DateTime.Now
            };

            await _logRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<(List<object> Logs, int Total)> GetLogListAsync(
            int pageIndex, int pageSize,
            string? keyword = null, string? module = null,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _logRepository.GetQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => l.Description != null && l.Description.Contains(keyword));

            if (!string.IsNullOrEmpty(module))
                query = query.Where(l => l.Module == module);

            if (startDate.HasValue)
                query = query.Where(l => l.CreateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreateTime <= endDate.Value.AddDays(1));

            var total = await System.Threading.Tasks.Task.Run(() => query.Count());
            var logs = await System.Threading.Tasks.Task.Run(() =>
                query.OrderByDescending(l => l.CreateTime)
                     .Skip((pageIndex - 1) * pageSize)
                     .Take(pageSize)
                     .ToList());

            return (logs.Cast<object>().ToList(), total);
        }
    }
}
