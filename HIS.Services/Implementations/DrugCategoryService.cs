using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class DrugCategoryService : IDrugCategoryService
    {
        private readonly IDrugCategoryRepository _repo;
        private readonly IUnitOfWork _unitOfWork;

        public DrugCategoryService(IDrugCategoryRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DrugCategoryDto>> GetTreeAsync()
        {
            var roots = await _repo.GetTreeAsync();
            return roots.Select(MapNode).ToList();
        }

        public async Task<List<DrugCategoryDto>> GetAllAsync()
        {
            var all = await _repo.GetAllOrderedAsync();
            return all.Select(c => new DrugCategoryDto
            {
                Id = c.Id, CategoryName = c.CategoryName,
                ParentId = c.ParentId, ParentName = c.Parent?.CategoryName,
                SortOrder = c.SortOrder
            }).ToList();
        }

        /// <summary>返回层级缩进的下拉列表（纯空格缩进，可搜索）</summary>
        public async Task<List<DrugCategoryDto>> GetHierarchicalAsync()
        {
            var all = await _repo.GetAllOrderedAsync();
            return BuildHierarchy(all, null, 0);
        }

        private List<DrugCategoryDto> BuildHierarchy(List<DrugCategory> all, long? parentId, int depth)
        {
            var result = new List<DrugCategoryDto>();
            var children = all.Where(c => c.ParentId == parentId).OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var c in children)
            {
                // 只用空格缩进，不加特殊符号，保证搜索功能正常
                var prefix = depth == 0 ? "" : new string(' ', depth * 4); // &nbsp; 缩进
                result.Add(new DrugCategoryDto
                {
                    Id = c.Id,
                    CategoryName = prefix + c.CategoryName,
                    ParentId = c.ParentId,
                    SortOrder = c.SortOrder,
                    Children = new()
                });
                result.AddRange(BuildHierarchy(all, c.Id, depth + 1));
            }
            return result;
        }

        public async Task<DrugCategoryDto?> GetByIdAsync(long id)
        {
            var c = await _repo.GetByIdAsync(id);
            return c == null ? null : new DrugCategoryDto
            {
                Id = c.Id, CategoryName = c.CategoryName,
                ParentId = c.ParentId, SortOrder = c.SortOrder
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(DrugCategoryDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            if (string.IsNullOrWhiteSpace(dto.CategoryName)) return (false, "分类名称不能为空");

            // 防止无限层级：父级不能是自己的子级
            if (dto.ParentId.HasValue)
            {
                var parent = await _repo.GetByIdAsync(dto.ParentId.Value);
                if (parent == null) return (false, "上级分类不存在");
            }

            var cat = new DrugCategory
            {
                CategoryName = dto.CategoryName,
                ParentId = dto.ParentId,
                SortOrder = dto.SortOrder
            };
            await _repo.AddAsync(cat);
            await _unitOfWork.SaveChangesAsync();
            return (true, "分类添加成功");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(DrugCategoryDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var cat = await _repo.GetByIdAsync(dto.Id);
            if (cat == null) return (false, "分类不存在");
            if (string.IsNullOrWhiteSpace(dto.CategoryName)) return (false, "分类名称不能为空");

            // 不能把自己设为父级
            if (dto.ParentId == dto.Id) return (false, "不能把自己设为上级分类");

            cat.CategoryName = dto.CategoryName;
            cat.ParentId = dto.ParentId;
            cat.SortOrder = dto.SortOrder;
            _repo.Update(cat);
            await _unitOfWork.SaveChangesAsync();
            return (true, "分类更新成功");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(long id)
        {
            var cat = await _repo.GetByIdAsync(id);
            if (cat == null) return (false, "分类不存在");

            if (await _repo.HasChildrenAsync(id))
                return (false, "该分类下有子分类，请先删除子分类");

            if (await _repo.HasDrugsAsync(id))
                return (false, "该分类下还有药品，请先移除药品");

            _repo.Delete(cat);
            await _unitOfWork.SaveChangesAsync();
            return (true, "分类已删除");
        }

        private static DrugCategoryDto MapNode(DrugCategory c) => new()
        {
            Id = c.Id, CategoryName = c.CategoryName,
            ParentId = c.ParentId, SortOrder = c.SortOrder,
            DrugCount = c.Drugs?.Count ?? 0,
            Children = c.Children?.Select(MapNode).ToList() ?? new()
        };
    }
}
