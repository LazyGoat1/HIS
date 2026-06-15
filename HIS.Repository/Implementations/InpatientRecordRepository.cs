using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class InpatientRecordRepository : BaseRepository<InpatientRecord>, IInpatientRecordRepository
    {
        public InpatientRecordRepository(HisDbContext context) : base(context) { }

        public async Task<(List<InpatientRecord> List, int Total)> GetListAsync(InpatientQueryModel query)
        {
            var q = _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Department)
                .Include(r => r.Doctor)
                .Include(r => r.Bed)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
                q = q.Where(r => r.InpatientNo.Contains(query.Keyword) ||
                    (r.Patient != null && r.Patient.Name.Contains(query.Keyword)));

            if (query.Status.HasValue)
                q = q.Where(r => r.Status == query.Status.Value);

            if (query.DepartmentId.HasValue)
                q = q.Where(r => r.DepartmentId == query.DepartmentId.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(r => r.DoctorId == query.DoctorId.Value);

            if (query.AdmissionStart.HasValue)
                q = q.Where(r => r.AdmissionTime >= query.AdmissionStart.Value);

            if (query.AdmissionEnd.HasValue)
                q = q.Where(r => r.AdmissionTime <= query.AdmissionEnd.Value);

            var total = await q.CountAsync();
            var list = await q.OrderByDescending(r => r.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return (list, total);
        }

        public async Task<InpatientRecord?> GetByInpatientNoAsync(string inpatientNo)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.InpatientNo == inpatientNo);
        }

        public async Task<InpatientRecord?> GetDetailAsync(long id)
        {
            return await _dbSet
                .Include(r => r.Patient).Include(r => r.Department)
                .Include(r => r.Doctor).Include(r => r.Bed)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> HasActiveRecordAsync(long patientId)
        {
            return await _dbSet.AnyAsync(r =>
                r.PatientId == patientId && r.Status == 1);
        }
    }
}
