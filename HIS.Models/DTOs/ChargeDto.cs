namespace HIS.Models.DTOs
{
    public class ChargeRecordDto
    {
        public long Id { get; set; }
        public string ChargeNo { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public int ChargeType { get; set; }
        public long? RelatedId { get; set; }
        public string? RelatedNo { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int PaymentMethod { get; set; }
        public int Status { get; set; }
        public long CreateUserId { get; set; }
        public string? CreateUserName { get; set; }
        public string? Remark { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class ChargeCreateDto
    {
        public long PatientId { get; set; }
        public int ChargeType { get; set; }
        public long? RelatedId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int PaymentMethod { get; set; }
        public string? Remark { get; set; }
    }
}
