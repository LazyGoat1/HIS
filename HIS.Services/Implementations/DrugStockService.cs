using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class DrugStockService : IDrugStockService
    {
        private readonly IDrugStockRepository _stockRepo;
        private readonly IDrugRepository _drugRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DrugStockService(IDrugStockRepository stockRepo, IDrugRepository drugRepo, IUnitOfWork unitOfWork)
        {
            _stockRepo = stockRepo;
            _drugRepo = drugRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<DrugStockLogDto> List, int Total)> GetListAsync(DrugStockQueryModel query)
        {
            var (list, total) = await _stockRepo.GetListAsync(query);
            return (list.Select(MapToDto).ToList(), total);
        }

        public async Task<(bool Success, string Message)> StockCheckAsync(long drugId, int actualQty, long userId)
        {
            var drug = await _drugRepo.GetByIdAsync(drugId);
            if (drug == null) return (false, "药品不存在");
            var beforeQty = drug.StockQuantity;
            var diff = actualQty - beforeQty;
            drug.StockQuantity = actualQty;
            _drugRepo.Update(drug);
            await _stockRepo.AddAsync(new DrugStockLog
            { DrugId = drugId, ChangeType = 5, ChangeQuantity = diff, BeforeQuantity = beforeQty, AfterQuantity = actualQty, RelatedNo = $"PD{DateTime.Now:yyyyMMddHHmmss}", CreateUserId = userId, CreateTime = DateTime.Now, Remark = $"盘点{(diff >= 0 ? "盈" : "亏")}{Math.Abs(diff)}" });
            await _unitOfWork.SaveChangesAsync();
            return (true, $"盘点完成：系统{beforeQty}，实际{actualQty}，{(diff >= 0 ? "盘盈" : "盘亏")}{Math.Abs(diff)}");
        }

        public async Task<List<DrugStockLogDto>> GetByDrugIdAsync(long drugId)
        {
            var logs = await _stockRepo.FindAsync(s => s.DrugId == drugId);
            return logs.OrderByDescending(s => s.CreateTime).Select(MapToDto).ToList();
        }

        public async Task<(bool Success, string Message)> StockInAsync(StockInDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            if (dto.Quantity <= 0) return (false, "入库数量必须大于0");

            var drug = await _drugRepo.GetByIdAsync(dto.DrugId);
            if (drug == null) return (false, "药品不存在");

            var beforeQty = drug.StockQuantity;
            drug.StockQuantity += dto.Quantity;
            _drugRepo.Update(drug);

            var log = new DrugStockLog
            {
                DrugId = dto.DrugId,
                ChangeType = 1, // 入库
                ChangeQuantity = dto.Quantity,
                BeforeQuantity = beforeQty,
                AfterQuantity = drug.StockQuantity,
                RelatedNo = dto.RelatedNo ?? GenerateNoHelper.GenerateChargeNo(),
                Remark = dto.Remark,
                CreateUserId = userId,
                CreateTime = DateTime.Now
            };
            await _stockRepo.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"入库成功：{drug.DrugName} +{dto.Quantity}，当前库存 {drug.StockQuantity}");
        }

        public async Task<(bool Success, string Message)> StockOutAsync(StockOutDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            if (dto.Quantity <= 0) return (false, "出库数量必须大于0");

            var drug = await _drugRepo.GetByIdAsync(dto.DrugId);
            if (drug == null) return (false, "药品不存在");

            if (drug.StockQuantity < dto.Quantity)
                return (false, $"库存不足：当前库存 {drug.StockQuantity}，需要 {dto.Quantity}");

            var beforeQty = drug.StockQuantity;
            drug.StockQuantity -= dto.Quantity;
            _drugRepo.Update(drug);

            var log = new DrugStockLog
            {
                DrugId = dto.DrugId,
                ChangeType = 2, // 出库
                ChangeQuantity = -dto.Quantity,
                BeforeQuantity = beforeQty,
                AfterQuantity = drug.StockQuantity,
                RelatedNo = dto.RelatedNo ?? GenerateNoHelper.GenerateChargeNo(),
                Remark = dto.Remark,
                CreateUserId = userId,
                CreateTime = DateTime.Now
            };
            await _stockRepo.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"出库成功：{drug.DrugName} -{dto.Quantity}，当前库存 {drug.StockQuantity}");
        }

        private static DrugStockLogDto MapToDto(DrugStockLog s) => new()
        {
            Id = s.Id, DrugId = s.DrugId,
            DrugName = s.Drug?.DrugName, DrugCode = s.Drug?.DrugCode,
            ChangeType = s.ChangeType, ChangeQuantity = s.ChangeQuantity,
            BeforeQuantity = s.BeforeQuantity, AfterQuantity = s.AfterQuantity,
            RelatedNo = s.RelatedNo, Remark = s.Remark,
            CreateUserId = s.CreateUserId, CreateTime = s.CreateTime
        };
    }
}
