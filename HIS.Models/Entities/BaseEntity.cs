using System.ComponentModel.DataAnnotations;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>主键</summary>
        [Key]
        public long Id { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 带软删除的实体基类
    /// </summary>
    public abstract class SoftDeleteEntity : BaseEntity
    {
        /// <summary>是否删除</summary>
        public bool IsDeleted { get; set; } = false;
    }
}
