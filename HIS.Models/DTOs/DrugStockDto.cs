namespace HIS.Models.DTOs
{
    public class DrugStockLogDto
    {
        public long Id { get; set; }
        public long DrugId { get; set; }
        public string? DrugName { get; set; }
        public string? DrugCode { get; set; }
        public int ChangeType { get; set; }
        public int ChangeQuantity { get; set; }
        public int BeforeQuantity { get; set; }
        public int AfterQuantity { get; set; }
        public string? RelatedNo { get; set; }
        public string? Remark { get; set; }
        public long CreateUserId { get; set; }
        public string? CreateUserName { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class StockInDto
    {
        public long DrugId { get; set; }
        public int Quantity { get; set; }
        public string? RelatedNo { get; set; }
        public string? Remark { get; set; }
    }

    public class StockOutDto
    {
        public long DrugId { get; set; }
        public int Quantity { get; set; }
        public string? RelatedNo { get; set; }
        public string? Remark { get; set; }
    }
}
