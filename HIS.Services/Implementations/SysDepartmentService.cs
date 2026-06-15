using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class SysDepartmentService : ISysDepartmentService
    {
        private readonly ISysDepartmentRepository _deptRepository;
        private readonly IBaseRepository<DoctorInfo> _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SysDepartmentService(ISysDepartmentRepository deptRepository, IBaseRepository<DoctorInfo> doctorRepository, IUnitOfWork unitOfWork)
        {
            _deptRepository = deptRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<SysDepartmentDto> Departments, int Total)> GetDepartmentListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var (depts, total) = await _deptRepository.GetDepartmentListAsync(pageIndex, pageSize, keyword);
            var dtos = depts.Select(MapToDto).ToList();
            return (dtos, total);
        }

        public async Task<List<SysDepartmentDto>> GetAllEnabledAsync()
        {
            var depts = await _deptRepository.GetAllEnabledAsync();
            return depts.Select(MapToDto).ToList();
        }

        public async Task<List<SysDepartmentDto>> GetDepartmentTreeAsync()
        {
            var depts = await _deptRepository.GetDepartmentTreeAsync();
            return BuildTree(depts, null);
        }

        public async Task<SysDepartmentDto?> GetDepartmentByIdAsync(long id)
        {
            var dept = await _deptRepository.GetByIdAsync(id);
            return dept == null ? null : MapToDto(dept);
        }

        public async Task<(bool Success, string Message)> CreateDepartmentAsync(SysDepartmentDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            if (await _deptRepository.AnyAsync(d => d.DeptCode == dto.DeptCode))
                return (false, "科室编码已存在");

            var dept = new SysDepartment
            {
                DeptName = dto.DeptName,
                DeptCode = dto.DeptCode,
                ParentId = dto.ParentId,
                DeptType = dto.DeptType,
                Phone = dto.Phone,
                Location = dto.Location,
                Description = dto.Description,
                SortOrder = dto.SortOrder,
                Status = dto.Status
            };

            await _deptRepository.AddAsync(dept);
            await _unitOfWork.SaveChangesAsync();
            return (true, "新增科室成功");
        }

        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(SysDepartmentDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var dept = await _deptRepository.GetByIdAsync(dto.Id);
            if (dept == null) return (false, "科室不存在");

            if (await _deptRepository.AnyAsync(d => d.DeptCode == dto.DeptCode && d.Id != dto.Id))
                return (false, "科室编码已存在");

            dept.DeptName = dto.DeptName;
            dept.DeptCode = dto.DeptCode;
            dept.ParentId = dto.ParentId;
            dept.DeptType = dto.DeptType;
            dept.Phone = dto.Phone;
            dept.Location = dto.Location;
            dept.Description = dto.Description;
            dept.SortOrder = dto.SortOrder;
            dept.Status = dto.Status;

            _deptRepository.Update(dept);
            await _unitOfWork.SaveChangesAsync();
            return (true, "更新科室成功");
        }

        public async Task<(bool Success, string Message)> DeleteDepartmentAsync(long id)
        {
            var dept = await _deptRepository.GetByIdAsync(id);
            if (dept == null) return (false, "科室不存在");

            // 检查是否有子科室
            if (await _deptRepository.AnyAsync(d => d.ParentId == id))
                return (false, "请先删除子科室");

            // 检查是否有医生
            if (await _doctorRepository.AnyAsync(d => d.DepartmentId == id))
                return (false, "该科室下还有医生，无法删除");

            _deptRepository.Delete(dept);
            await _unitOfWork.SaveChangesAsync();
            return (true, "删除科室成功");
        }

        private static SysDepartmentDto MapToDto(SysDepartment dept) => new()
        {
            Id = dept.Id,
            DeptName = dept.DeptName,
            DeptCode = dept.DeptCode,
            ParentId = dept.ParentId,
            DeptType = dept.DeptType,
            Phone = dept.Phone,
            Location = dept.Location,
            Description = dept.Description,
            SortOrder = dept.SortOrder,
            Status = dept.Status,
            CreateTime = dept.CreateTime
        };

        private List<SysDepartmentDto> BuildTree(List<SysDepartment> depts, long? parentId)
        {
            return depts
                .Where(d => d.ParentId == parentId)
                .Select(MapToDto)
                .ToList();
        }
    }
}
