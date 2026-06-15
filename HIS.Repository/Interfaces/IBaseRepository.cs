using System.Linq.Expressions;
using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 基础仓储接口
    /// </summary>
    public interface IBaseRepository<T> where T : BaseEntity
    {
        /// <summary>根据ID获取</summary>
        Task<T?> GetByIdAsync(long id);

        /// <summary>获取所有数据</summary>
        Task<List<T>> GetAllAsync();

        /// <summary>带条件查询</summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>带条件查询第一个</summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>分页查询</summary>
        Task<(List<T> Items, int Total)> GetPagedAsync(int pageIndex, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        /// <summary>是否存在</summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>计数</summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>新增</summary>
        Task<T> AddAsync(T entity);

        /// <summary>批量新增</summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>更新</summary>
        void Update(T entity);

        /// <summary>批量更新</summary>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>删除</summary>
        void Delete(T entity);

        /// <summary>批量删除</summary>
        void DeleteRange(IEnumerable<T> entities);

        /// <summary>软删除（仅适用于继承SoftDeleteEntity的实体）</summary>
        void SoftDelete(T entity);

        /// <summary>获取IQueryable</summary>
        IQueryable<T> GetQueryable();
    }
}
