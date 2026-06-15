namespace HIS.Models.QueryModels
{
    public class DrugStockQueryModel
    {
        public string? Keyword { get; set; }
        public long? DrugId { get; set; }
        public int? ChangeType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
