namespace HIS.Models.DTOs
{
    /// <summary>
    /// 住院记录DTO
    /// </summary>
    public class InpatientRecordDto
    {
        public long Id { get; set; }
        public string InpatientNo { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public long? BedId { get; set; }
        public string? BedNo { get; set; }
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public long DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime AdmissionTime { get; set; }
        public DateTime? DischargeTime { get; set; }
        public string? AdmissionDiagnosis { get; set; }
        public string? DischargeDiagnosis { get; set; }
        public int Status { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 床位DTO
    /// </summary>
    public class BedInfoDto
    {
        public long Id { get; set; }
        public string BedNo { get; set; } = string.Empty;
        public string RoomNo { get; set; } = string.Empty;
        public long DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int BedType { get; set; }
        public decimal DailyRate { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 医嘱DTO
    /// </summary>
    public class MedicalOrderDto
    {
        public long Id { get; set; }
        public long InpatientId { get; set; }
        public string? InpatientNo { get; set; }
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public long DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public int OrderType { get; set; }
        public string OrderContent { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Status { get; set; }
        public long? ExecutorId { get; set; }
        public string? ExecutorName { get; set; }
        public DateTime? ExecuteTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
