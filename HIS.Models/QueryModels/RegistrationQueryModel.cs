namespace HIS.Models.QueryModels
{
    /// <summary>
    /// 挂号查询模型
    /// </summary>
    public class RegistrationQueryModel
    {
        /// <summary>搜索关键词：挂号单号/患者姓名</summary>
        public string? Keyword { get; set; }

        /// <summary>就诊日期 - 开始</summary>
        public DateTime? VisitDateStart { get; set; }

        /// <summary>就诊日期 - 结束</summary>
        public DateTime? VisitDateEnd { get; set; }

        /// <summary>挂号类型 1:普通 2:专家 3:急诊</summary>
        public int? RegistrationType { get; set; }

        /// <summary>状态 1:已挂号 2:已接诊 3:已退号</summary>
        public int? Status { get; set; }

        /// <summary>科室ID</summary>
        public long? DepartmentId { get; set; }

        /// <summary>医生ID</summary>
        public long? DoctorId { get; set; }

        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>每页条数</summary>
        public int PageSize { get; set; } = 10;
    }
}
