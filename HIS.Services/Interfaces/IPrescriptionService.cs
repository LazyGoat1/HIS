using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 处方服务接口
    /// </summary>
    public interface IPrescriptionService
    {
        /// <summary>分页查询处方列表</summary>
        Task<(List<PrescriptionDto> List, int Total)> GetListAsync(PrescriptionQueryModel query);

        /// <summary>根据ID获取处方详情（含明细）</summary>
        Task<PrescriptionDto?> GetByIdAsync(long id);

        /// <summary>开具处方（含明细）</summary>
        Task<(bool Success, string Message)> CreateAsync(PrescriptionDto dto, long userId);

        /// <summary>根据门诊记录ID获取处方列表</summary>
        Task<List<PrescriptionDto>> GetByOutpatientRecordIdAsync(long outpatientRecordId);

        /// <summary>获取待发药处方列表</summary>
        Task<(List<PrescriptionDto> List, int Total)> GetPendingDispenseAsync(PrescriptionQueryModel query);

        /// <summary>确认发药（审方通过 → 扣库存 → 改状态）</summary>
        Task<(bool Success, string Message)> DispenseAsync(long prescriptionId, long userId);

        /// <summary>退药（已发药 → 库存回加 → 改状态为已退方）</summary>
        Task<(bool Success, string Message)> ReturnAsync(long prescriptionId, long userId);
    }
}
