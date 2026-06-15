using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 处方仓储接口
    /// </summary>
    public interface IPrescriptionRepository : IBaseRepository<Prescription>
    {
        /// <summary>
        /// 分页查询处方列表
        /// </summary>
        Task<(List<Prescription> Prescriptions, int Total)> GetListAsync(PrescriptionQueryModel query);

        /// <summary>
        /// 根据处方号查找
        /// </summary>
        Task<Prescription?> GetByPrescriptionNoAsync(string prescriptionNo);

        /// <summary>
        /// 获取处方详情（包含明细和患者/医生信息）
        /// </summary>
        Task<Prescription?> GetDetailAsync(long id);

        /// <summary>
        /// 更新处方状态
        /// </summary>
        Task UpdateStatusAsync(long id, int status);
    }
}
