namespace HIS.Models.QueryModels
{
    public class InpatientQueryModel
    {
        public string? Keyword { get; set; }
        public int? Status { get; set; }
        public long? DepartmentId { get; set; }
        public long? DoctorId { get; set; }
        public DateTime? AdmissionStart { get; set; }
        public DateTime? AdmissionEnd { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class BedQueryModel
    {
        public string? Keyword { get; set; }
        public long? DepartmentId { get; set; }
        public int? Status { get; set; }
        public int? BedType { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class MedicalOrderQueryModel
    {
        public long? InpatientId { get; set; }
        public string? Keyword { get; set; }
        public int? OrderType { get; set; }
        public int? Status { get; set; }
        public long? DoctorId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
