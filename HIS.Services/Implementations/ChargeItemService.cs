using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Services.Implementations
{
    public class ChargeItemService : IChargeItemService
    {
        private readonly IChargeItemRepository _repo;
        private readonly IUnitOfWork _uow;

        public ChargeItemService(IChargeItemRepository repo, IUnitOfWork uow)
        { _repo = repo; _uow = uow; }

        public async Task<List<ChargeItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllEnabledAsync();
            return items.Select(Map).ToList();
        }

        public async Task<(List<ChargeItemDto> List, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null)
        {
            var (list, total) = await _repo.GetPagedAsync(pageIndex, pageSize, keyword);
            return (list.Select(Map).ToList(), total);
        }

        public async Task<List<ChargeItemDto>> GetByCategoryAsync(string category)
        {
            var items = await _repo.GetByCategoryAsync(category);
            return items.Select(Map).ToList();
        }

        public async Task<List<string>> GetCategoriesAsync()
            => await _repo.GetCategoriesAsync();

        public async Task<ChargeItemDto?> GetByIdAsync(long id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : Map(item);
        }

        public async Task<(bool, string)> CreateAsync(ChargeItemDto dto)
        {
            if (dto == null) return (false, "数据为空");
            if (string.IsNullOrWhiteSpace(dto.ItemName)) return (false, "项目名称不能为空");
            await _repo.AddAsync(new ChargeItem
            { Category = dto.Category, ItemName = dto.ItemName, Specification = dto.Specification,
                Unit = dto.Unit ?? "次", UnitPrice = dto.UnitPrice, Description = dto.Description, Status = 1 });
            await _uow.SaveChangesAsync();
            return (true, "添加成功");
        }

        public async Task<(bool, string)> UpdateAsync(ChargeItemDto dto)
        {
            if (dto == null) return (false, "数据为空");
            var item = await _repo.GetByIdAsync(dto.Id);
            if (item == null) return (false, "项目不存在");
            item.Category = dto.Category; item.ItemName = dto.ItemName;
            item.Specification = dto.Specification; item.Unit = dto.Unit;
            item.UnitPrice = dto.UnitPrice; item.Description = dto.Description;
            _repo.Update(item); await _uow.SaveChangesAsync();
            return (true, "更新成功");
        }

        public async Task<(bool, string)> DeleteAsync(long id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return (false, "项目不存在");
            _repo.Delete(item); await _uow.SaveChangesAsync();
            return (true, "已删除");
        }

        private static ChargeItemDto Map(ChargeItem c) => new()
        { Id = c.Id, Category = c.Category, ItemName = c.ItemName,
            Specification = c.Specification, Unit = c.Unit, UnitPrice = c.UnitPrice,
            Description = c.Description, Status = c.Status, CreateTime = c.CreateTime };
    }
}
