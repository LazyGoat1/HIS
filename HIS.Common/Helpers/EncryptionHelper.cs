using System.Security.Cryptography;
using System.Text;

namespace HIS.Common.Helpers
{
    /// <summary>
    /// 加密工具类
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// SHA256 哈希（用于密码存储）
        /// </summary>
        public static string Sha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// 密码加盐哈希
        /// </summary>
        public static string HashPassword(string password, string salt)
        {
            return Sha256Hash(password + salt);
        }

        /// <summary>
        /// 生成随机盐值
        /// </summary>
        public static string GenerateSalt(int length = 16)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// MD5哈希（用于简单校验）
        /// </summary>
        public static string Md5Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
