using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 患者信息
    /// </summary>
    [Table("PatientInfo")]
    public class PatientInfo : BaseEntity
    {
        /// <summary>患者编号</summary>
        [Required, MaxLength(50)]
        public string PatientNo { get; set; } = string.Empty;

        /// <summary>姓名</summary>
        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        /// <summary>性别 0:女 1:男</summary>
        public int Gender { get; set; }

        /// <summary>出生日期</summary>
        public DateTime? Birthday { get; set; }

        /// <summary>年龄</summary>
        public int? Age { get; set; }

        /// <summary>身份证号</summary>
        [MaxLength(18)]
        public string? IdCard { get; set; }

        /// <summary>手机号</summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>地址</summary>
        [MaxLength(200)]
        public string? Address { get; set; }

        /// <summary>血型</summary>
        [MaxLength(10)]
        public string? BloodType { get; set; }

        /// <summary>过敏史</summary>
        [MaxLength(500)]
        public string? AllergyHistory { get; set; }

        // 导航属性
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<OutpatientRecord> OutpatientRecords { get; set; } = new List<OutpatientRecord>();
    }
}
