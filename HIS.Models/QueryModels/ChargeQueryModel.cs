namespace HIS.Models.QueryModels
{
    public class ChargeQueryModel
    {
        public string? Keyword { get; set; }
        public int? ChargeType { get; set; }
        public int? Status { get; set; }
        public int? PaymentMethod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
