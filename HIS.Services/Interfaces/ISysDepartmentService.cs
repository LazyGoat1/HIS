using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    public interface ISysDepartmentService
    {
        Task<(List<SysDepartmentDto> Departments, int Total)> GetDepartmentListAsync(int pageIndex, int pageSize, string? keyword = null);
        Task<List<SysDepartmentDto>> GetAllEnabledAsync();
        Task<List<SysDepartmentDto>> GetDepartmentTreeAsync();
        Task<SysDepartmentDto?> GetDepartmentByIdAsync(long id);
        Task<(bool Success, string Message)> CreateDepartmentAsync(SysDepartmentDto dto);
        Task<(bool Success, string Message)> UpdateDepartmentAsync(SysDepartmentDto dto);
        Task<(bool Success, string Message)> DeleteDepartmentAsync(long id);
    }
}
