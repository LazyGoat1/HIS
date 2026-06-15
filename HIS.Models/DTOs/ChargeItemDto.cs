namespace HIS.Models.DTOs
{
    public class ChargeItemDto
    {
        public long Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? Specification { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
