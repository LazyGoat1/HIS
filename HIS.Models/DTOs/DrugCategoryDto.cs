namespace HIS.Models.DTOs
{
    public class DrugCategoryDto
    {
        public long Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public int DrugCount { get; set; }
        public List<DrugCategoryDto> Children { get; set; } = new();
    }
}
