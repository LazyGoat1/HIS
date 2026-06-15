using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 操作日志
    /// </summary>
    [Table("SysLog")]
    public class SysLog : BaseEntity
    {
        /// <summary>操作用户ID</summary>
        public long? UserId { get; set; }

        /// <summary>操作用户名</summary>
        [MaxLength(50)]
        public string? UserName { get; set; }

        /// <summary>操作模块</summary>
        [MaxLength(50)]
        public string Module { get; set; } = string.Empty;

        /// <summary>操作类型</summary>
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>操作描述</summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>请求URL</summary>
        [MaxLength(500)]
        public string? RequestUrl { get; set; }

        /// <summary>IP地址</summary>
        [MaxLength(50)]
        public string? IPAddress { get; set; }

        /// <summary>操作耗时(ms)</summary>
        public long? Elapsed { get; set; }
    }
}
