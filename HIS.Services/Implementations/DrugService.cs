using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 药品字典服务实现
    /// 核心业务逻辑：
    /// 1. 新增时自动生成药品编码（YP + 8位流水号）
    /// 2. 药品编码全局唯一校验
    /// 3. 库存管理独立于基本信息（入库/出库走 DrugStockLog）
    /// 4. 库存低于最低库存时触发预警
    /// 5. 处方药标记控制发药流程
    /// </summary>
    public class DrugService : IDrugService
    {
        private readonly IDrugRepository _drugRepository;
        private readonly IBaseRepository<DrugStockLog> _stockLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DrugService(
            IDrugRepository drugRepository,
            IBaseRepository<DrugStockLog> stockLogRepository,
            IUnitOfWork unitOfWork)
        {
            _drugRepository = drugRepository;
            _stockLogRepository = stockLogRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询药品列表
        /// </summary>
        public async Task<(List<DrugDto> Drugs, int Total)> GetDrugListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? categoryId = null)
        {
            var (drugs, total) = await _drugRepository.GetDrugListAsync(
                pageIndex, pageSize, keyword, categoryId);
            var dtos = drugs.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取药品详情
        /// </summary>
        public async Task<DrugDto?> GetDrugByIdAsync(long id)
        {
            // 使用 GetQueryable 来加载分类导航属性
            var drug = await System.Threading.Tasks.Task.Run(() =>
                _drugRepository.GetQueryable()
                    .Where(d => d.Id == id)
                    .Select(d => MapToDto(d))
                    .FirstOrDefault());
            return drug;
        }

        /// <summary>
        /// 新增药品
        /// 业务规则：
        /// - 自动生成药品编码
        /// - 初始库存设为0，后续通过入库操作增加
        /// - 零售价必须大于进货价（防止亏损）
        /// </summary>
        public async Task<(bool Success, string Message)> CreateDrugAsync(DrugDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            // 生成药品编码
            var currentCount = await _drugRepository.CountAsync();
            var drugCode = GenerateNoHelper.GenerateDrugCode(currentCount);

            // 编码唯一性校验
            if (await _drugRepository.GetByDrugCodeAsync(drugCode) != null)
                return (false, "药品编码生成冲突，请重试");

            // 零售价合理性校验
            if (dto.RetailPrice < dto.UnitPrice)
                return (false, "零售价不能低于进货价");

            var drug = new DrugInfo
            {
                DrugCode = drugCode,
                DrugName = dto.DrugName,
                GenericName = dto.GenericName,
                CategoryId = dto.CategoryId,
                Specification = dto.Specification,
                Unit = dto.Unit,
                Manufacturer = dto.Manufacturer,
                UnitPrice = dto.UnitPrice,       // 进货价
                RetailPrice = dto.RetailPrice,   // 零售价
                StockQuantity = 0,               // 初始库存为0
                MinStock = dto.MinStock,
                IsPrescription = dto.IsPrescription,
                Status = dto.Status,
                CreateTime = DateTime.Now
            };

            await _drugRepository.AddAsync(drug);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"药品添加成功，编码：{drugCode}");
        }

        /// <summary>
        /// 更新药品基本信息
        /// 注意：库存数量不能在这里直接修改，必须走入库/出库流程
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateDrugAsync(DrugDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var drug = await _drugRepository.GetByIdAsync(dto.Id);
            if (drug == null)
                return (false, "药品不存在");

            // 零售价合理性
            if (dto.RetailPrice < dto.UnitPrice)
                return (false, "零售价不能低于进货价");

            drug.DrugName = dto.DrugName;
            drug.GenericName = dto.GenericName;
            drug.CategoryId = dto.CategoryId;
            drug.Specification = dto.Specification;
            drug.Unit = dto.Unit;
            drug.Manufacturer = dto.Manufacturer;
            drug.UnitPrice = dto.UnitPrice;
            drug.RetailPrice = dto.RetailPrice;
            drug.MinStock = dto.MinStock;
            drug.IsPrescription = dto.IsPrescription;
            // 注意：StockQuantity 不在此处修改，库存变更通过 DrugStockLog

            _drugRepository.Update(drug);
            await _unitOfWork.SaveChangesAsync();

            return (true, "药品信息更新成功");
        }

        /// <summary>
        /// 删除药品
        /// 业务规则：检查是否有关联的库存操作日志
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteDrugAsync(long id)
        {
            var drug = await _drugRepository.GetByIdAsync(id);
            if (drug == null)
                return (false, "药品不存在");

            // 有库存时禁止删除（需先做退货/出库处理）
            if (drug.StockQuantity > 0)
                return (false, $"药品当前库存为 {drug.StockQuantity}，请先清空库存后再删除");

            _drugRepository.Delete(drug);
            await _unitOfWork.SaveChangesAsync();

            return (true, "药品信息已删除");
        }

        /// <summary>
        /// 获取库存预警列表
        /// 用于首页/药房管理展示需要补货的药品
        /// </summary>
        public async Task<List<DrugDto>> GetLowStockDrugsAsync()
        {
            var drugs = await _drugRepository.GetLowStockDrugsAsync();
            return drugs.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 停用/启用药品
        /// 停用后不可再开处方
        /// </summary>
        public async Task<(bool Success, string Message)> SetStatusAsync(long id, int status)
        {
            var drug = await _drugRepository.GetByIdAsync(id);
            if (drug == null)
                return (false, "药品不存在");

            drug.Status = status;
            _drugRepository.Update(drug);
            await _unitOfWork.SaveChangesAsync();

            var statusText = status == 1 ? "已启用" : "已停用";
            return (true, $"药品{statusText}");
        }

        #region 私有辅助方法

        /// <summary>
        /// 实体→DTO映射
        /// </summary>
        private static DrugDto MapToDto(DrugInfo d) => new()
        {
            Id = d.Id,
            DrugCode = d.DrugCode,
            DrugName = d.DrugName,
            GenericName = d.GenericName,
            CategoryId = d.CategoryId,
            CategoryName = d.Category?.CategoryName,
            Specification = d.Specification,
            Unit = d.Unit,
            Manufacturer = d.Manufacturer,
            UnitPrice = d.UnitPrice,
            RetailPrice = d.RetailPrice,
            StockQuantity = d.StockQuantity,
            MinStock = d.MinStock,
            IsPrescription = d.IsPrescription,
            Status = d.Status,
            CreateTime = d.CreateTime
        };

        #endregion
    }
}
