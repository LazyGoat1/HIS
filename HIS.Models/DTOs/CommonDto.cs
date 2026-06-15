namespace HIS.Models.DTOs
{
    /// <summary>
    /// 用户DTO
    /// </summary>
    public class SysUserDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public long? DepartmentId { get; set; }
        public long RoleId { get; set; }
        public int Status { get; set; }
    }

    /// <summary>
    /// 创建/编辑用户DTO
    /// </summary>
    public class SysUserCreateDto
    {
        public long? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string RealName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public long? DepartmentId { get; set; }
        public long RoleId { get; set; }
        public int Status { get; set; } = 1;
    }

    /// <summary>
    /// 患者DTO
    /// </summary>
    public class PatientDto
    {
        public long Id { get; set; }
        public string PatientNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Age { get; set; }
        public string? IdCard { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? BloodType { get; set; }
        public string? AllergyHistory { get; set; }
    }

    /// <summary>
    /// 医生DTO
    /// </summary>
    public class DoctorDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string DoctorNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Gender { get; set; }
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int Title { get; set; }
        public string? Specialty { get; set; }
        public int MaxDailyPatients { get; set; } = 50;
        public decimal ConsultationFee { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 挂号DTO
    /// </summary>
    public class RegistrationDto
    {
        public long Id { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public long DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public int RegistrationType { get; set; }
        public decimal RegistrationFee { get; set; }
        public int Status { get; set; }
        public DateTime VisitDate { get; set; }
        public int QueueNumber { get; set; }
    }

    /// <summary>
    /// 药品DTO
    /// </summary>
    public class DrugDto
    {
        public long Id { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public string? Manufacturer { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinStock { get; set; } = 10;
        public bool IsPrescription { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 处方DTO
    /// </summary>
    public class PrescriptionDto
    {
        public long Id { get; set; }
        public string PrescriptionNo { get; set; } = string.Empty;
        public long RegistrationId { get; set; }
        public long OutpatientRecordId { get; set; }
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public long DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public int PrescriptionType { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string? Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public List<PrescriptionDetailDto> Details { get; set; } = new();
    }

    /// <summary>
    /// 处方明细DTO
    /// </summary>
    public class PrescriptionDetailDto
    {
        public long Id { get; set; }
        public long PrescriptionId { get; set; }
        public int ItemType { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string? Usage { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? Days { get; set; }
    }
}
