using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 患者信息仓储实现
    /// </summary>
    public class PatientRepository : BaseRepository<PatientInfo>, IPatientRepository
    {
        public PatientRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询患者列表
        /// 支持按姓名、手机号、身份证号模糊搜索
        /// </summary>
        public async Task<(List<PatientInfo> Patients, int Total)> GetPatientListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var query = _dbSet.AsQueryable();

            // 关键字模糊匹配：患者姓名/手机号/身份证号/患者编号
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.Name.Contains(keyword) ||
                    (p.Phone != null && p.Phone.Contains(keyword)) ||
                    (p.IdCard != null && p.IdCard.Contains(keyword)) ||
                    p.PatientNo.Contains(keyword));
            }

            var total = await query.CountAsync();
            var patients = await query
                .OrderByDescending(p => p.CreateTime)  // 最近创建的患者排在前面
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (patients, total);
        }

        /// <summary>
        /// 根据身份证号查重（新增患者时避免重复建档）
        /// </summary>
        public async Task<PatientInfo?> GetByIdCardAsync(string idCard)
        {
            if (string.IsNullOrEmpty(idCard)) return null;
            return await _dbSet.FirstOrDefaultAsync(p => p.IdCard == idCard);
        }

        /// <summary>
        /// 根据手机号查重
        /// </summary>
        public async Task<PatientInfo?> GetByPhoneAsync(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return null;
            return await _dbSet.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        /// <summary>
        /// 根据患者编号精确查找
        /// </summary>
        public async Task<PatientInfo?> GetByPatientNoAsync(string patientNo)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PatientNo == patientNo);
        }
    }
}
