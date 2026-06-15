namespace HIS.Common.Helpers
{
    /// <summary>
    /// 业务编号生成器
    /// </summary>
    public static class GenerateNoHelper
    {
        private static readonly object _lock = new();
        private static int _dailySequence = 0;
        private static string _lastDate = string.Empty;

        /// <summary>
        /// 生成业务编号（日期+流水号）
        /// </summary>
        public static string GenerateNo(string prefix)
        {
            lock (_lock)
            {
                var today = DateTime.Now.ToString("yyyyMMdd");

                if (_lastDate != today)
                {
                    _lastDate = today;
                    _dailySequence = 0;
                }

                _dailySequence++;
                return $"{prefix}{today}{_dailySequence:D4}";
            }
        }

        /// <summary>
        /// 生成患者编号
        /// </summary>
        public static string GeneratePatientNo()
        {
            return $"P{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";
        }

        /// <summary>
        /// 生成挂号单号
        /// </summary>
        public static string GenerateRegistrationNo() => GenerateNo("GH");

        /// <summary>
        /// 生成处方号
        /// </summary>
        public static string GeneratePrescriptionNo() => GenerateNo("CF");

        /// <summary>
        /// 生成住院号
        /// </summary>
        public static string GenerateInpatientNo() => GenerateNo("ZY");

        /// <summary>
        /// 生成收费单号
        /// </summary>
        public static string GenerateChargeNo() => GenerateNo("SF");

        /// <summary>
        /// 生成医生工号
        /// </summary>
        public static string GenerateDoctorNo(int currentCount)
        {
            return $"YS{currentCount + 1:D4}";
        }

        /// <summary>
        /// 生成药品编码
        /// </summary>
        public static string GenerateDrugCode(int currentCount)
        {
            return $"YP{currentCount + 1:D8}";
        }
    }
}
