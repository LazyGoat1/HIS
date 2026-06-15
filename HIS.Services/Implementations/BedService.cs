using HIS.Common.Enums;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class BedService : IBedService
    {
        private readonly IBedInfoRepository _bedRepo;
        private readonly IBaseRepository<InpatientRecord> _inpatientRepo;
        private readonly IUnitOfWork _unitOfWork;

        public BedService(IBedInfoRepository bedRepo,
            IBaseRepository<InpatientRecord> inpatientRepo, IUnitOfWork unitOfWork)
        {
            _bedRepo = bedRepo;
            _inpatientRepo = inpatientRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<BedInfoDto> List, int Total)> GetListAsync(BedQueryModel query)
        {
            var (list, total) = await _bedRepo.GetListAsync(query);
            return (list.Select(MapToDto).ToList(), total);
        }

        public async Task<List<BedInfoDto>> GetAvailableBedsAsync(long departmentId)
        {
            var beds = await _bedRepo.GetAvailableBedsAsync(departmentId);
            return beds.Select(MapToDto).ToList();
        }

        public async Task<BedInfoDto?> GetByIdAsync(long id)
        {
            var bed = await _bedRepo.GetByIdAsync(id);
            return bed == null ? null : MapToDto(bed);
        }

        public async Task<(bool Success, string Message)> CreateAsync(BedInfoDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var exist = await _bedRepo.GetByBedNoAsync(dto.BedNo, dto.DepartmentId);
            if (exist != null) return (false, $"科室下已存在床位号 {dto.BedNo}");

            var bed = new BedInfo
            {
                BedNo = dto.BedNo, RoomNo = dto.RoomNo,
                DepartmentId = dto.DepartmentId, BedType = dto.BedType,
                DailyRate = dto.DailyRate, Status = (int)BedStatusEnum.Available,
                CreateTime = DateTime.Now
            };
            await _bedRepo.AddAsync(bed);
            await _unitOfWork.SaveChangesAsync();
            return (true, "床位添加成功");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(BedInfoDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var bed = await _bedRepo.GetByIdAsync(dto.Id);
            if (bed == null) return (false, "床位不存在");

            bed.BedNo = dto.BedNo; bed.RoomNo = dto.RoomNo;
            bed.DepartmentId = dto.DepartmentId; bed.BedType = dto.BedType;
            bed.DailyRate = dto.DailyRate;
            _bedRepo.Update(bed);
            await _unitOfWork.SaveChangesAsync();
            return (true, "床位更新成功");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(long id)
        {
            var bed = await _bedRepo.GetByIdAsync(id);
            if (bed == null) return (false, "床位不存在");
            if (bed.Status == (int)BedStatusEnum.Occupied)
                return (false, "床位正在使用中，无法删除");

            var hasRecord = await _inpatientRepo.AnyAsync(r => r.BedId == id);
            if (hasRecord) return (false, "该床位有住院记录关联，无法删除");

            _bedRepo.Delete(bed);
            await _unitOfWork.SaveChangesAsync();
            return (true, "床位已删除");
        }

        private static BedInfoDto MapToDto(BedInfo b) => new()
        {
            Id = b.Id, BedNo = b.BedNo, RoomNo = b.RoomNo,
            DepartmentId = b.DepartmentId, DepartmentName = b.Department?.DeptName,
            BedType = b.BedType, DailyRate = b.DailyRate, Status = b.Status,
            CreateTime = b.CreateTime
        };
    }
}
