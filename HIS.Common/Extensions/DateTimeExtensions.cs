namespace HIS.Common.Extensions
{
    /// <summary>
    /// DateTime 扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 转换为标准日期字符串 yyyy-MM-dd
        /// </summary>
        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 转换为标准日期时间字符串 yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string ToStandardString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取月份第一天
        /// </summary>
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// 获取月份最后一天
        /// </summary>
        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month,
                DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }

        /// <summary>
        /// 获取年龄
        /// </summary>
        public static int GetAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// 获取Unix时间戳(秒)
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }
    }
}
