using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 药品字典服务接口
    /// </summary>
    public interface IDrugService
    {
        /// <summary>分页查询药品列表</summary>
        Task<(List<DrugDto> Drugs, int Total)> GetDrugListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? categoryId = null);

        /// <summary>根据ID获取药品详情</summary>
        Task<DrugDto?> GetDrugByIdAsync(long id);

        /// <summary>新增药品</summary>
        Task<(bool Success, string Message)> CreateDrugAsync(DrugDto dto);

        /// <summary>更新药品信息（不包含库存变更，库存走专门的入库/出库接口）</summary>
        Task<(bool Success, string Message)> UpdateDrugAsync(DrugDto dto);

        /// <summary>删除药品（检查库存日志关联）</summary>
        Task<(bool Success, string Message)> DeleteDrugAsync(long id);

        /// <summary>获取库存预警药品列表</summary>
        Task<List<DrugDto>> GetLowStockDrugsAsync();

        /// <summary>停用/启用药品</summary>
        Task<(bool Success, string Message)> SetStatusAsync(long id, int status);
    }
}
