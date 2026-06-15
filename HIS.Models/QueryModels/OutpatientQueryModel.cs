namespace HIS.Models.QueryModels
{
    /// <summary>
    /// 门诊记录查询模型
    /// </summary>
    public class OutpatientQueryModel
    {
        /// <summary>搜索关键词：患者姓名/挂号单号</summary>
        public string? Keyword { get; set; }

        /// <summary>就诊日期 - 开始</summary>
        public DateTime? VisitDateStart { get; set; }

        /// <summary>就诊日期 - 结束</summary>
        public DateTime? VisitDateEnd { get; set; }

        /// <summary>医生ID</summary>
        public long? DoctorId { get; set; }

        /// <summary>科室ID</summary>
        public long? DepartmentId { get; set; }

        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>每页条数</summary>
        public int PageSize { get; set; } = 10;
    }
}
