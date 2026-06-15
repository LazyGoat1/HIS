namespace HIS.Common.Extensions
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判断字符串是否非空
        /// </summary>
        public static bool IsNotNullOrEmpty(this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 截取指定长度字符串
        /// </summary>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        /// <summary>
        /// 字符串转decimal
        /// </summary>
        public static decimal ToDecimal(this string value, decimal defaultValue = 0)
        {
            return decimal.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 字符串转int
        /// </summary>
        public static int ToInt(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 字符串转long
        /// </summary>
        public static long ToLong(this string value, long defaultValue = 0)
        {
            return long.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 隐藏手机号中间4位
        /// </summary>
        public static string MaskPhone(this string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length != 11) return phone;
            return phone[..3] + "****" + phone[7..];
        }

        /// <summary>
        /// 隐藏身份证号中间部分
        /// </summary>
        public static string MaskIdCard(this string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length < 8) return idCard;
            return idCard[..4] + "**********" + idCard[^4..];
        }
    }
}
