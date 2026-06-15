namespace HIS.Models.QueryModels
{
    /// <summary>
    /// 处方查询模型
    /// </summary>
    public class PrescriptionQueryModel
    {
        /// <summary>搜索关键词：处方号/患者姓名</summary>
        public string? Keyword { get; set; }

        /// <summary>处方类型 1:西药 2:中药 3:检查</summary>
        public int? PrescriptionType { get; set; }

        /// <summary>状态 1:已开具 2:已收费 3:已发药 4:已退方</summary>
        public int? Status { get; set; }

        /// <summary>开具日期 - 开始</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>开具日期 - 结束</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>医生ID</summary>
        public long? DoctorId { get; set; }

        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>每页条数</summary>
        public int PageSize { get; set; } = 10;
    }
}
