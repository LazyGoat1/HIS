namespace HIS.Repository.UnitOfWork
{
    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>保存更改</summary>
        int SaveChanges();

        /// <summary>异步保存更改</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>开始事务</summary>
        Task BeginTransactionAsync();

        /// <summary>提交事务</summary>
        Task CommitTransactionAsync();

        /// <summary>回滚事务</summary>
        Task RollbackTransactionAsync();
    }
}
