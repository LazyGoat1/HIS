namespace HIS.Models.DTOs
{
    /// <summary>
    /// 门诊诊疗记录DTO
    /// </summary>
    public class OutpatientRecordDto
    {
        public long Id { get; set; }
        public long RegistrationId { get; set; }
        public string? RegistrationNo { get; set; }
        public long PatientId { get; set; }
        public string? PatientName { get; set; }
        public long DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? PresentIllness { get; set; }
        public string? PastHistory { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? PreliminaryDiagnosis { get; set; }
        public string? Advice { get; set; }
        public DateTime VisitTime { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 门诊诊疗创建DTO（医生接诊时填写）
    /// </summary>
    public class OutpatientRecordCreateDto
    {
        /// <summary>挂号ID（必须）</summary>
        public long RegistrationId { get; set; }

        /// <summary>主诉</summary>
        public string? ChiefComplaint { get; set; }

        /// <summary>现病史</summary>
        public string? PresentIllness { get; set; }

        /// <summary>既往史</summary>
        public string? PastHistory { get; set; }

        /// <summary>体格检查</summary>
        public string? PhysicalExamination { get; set; }

        /// <summary>初步诊断</summary>
        public string? PreliminaryDiagnosis { get; set; }

        /// <summary>医嘱建议</summary>
        public string? Advice { get; set; }
    }
}
