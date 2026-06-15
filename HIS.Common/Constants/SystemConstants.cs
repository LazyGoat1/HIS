namespace HIS.Common.Constants
{
    /// <summary>
    /// 系统常量定义
    /// </summary>
    public static class SystemConstants
    {
        /// <summary>默认密码</summary>
        public const string DefaultPassword = "123456";

        /// <summary>密码最小长度</summary>
        public const int MinPasswordLength = 6;

        /// <summary>密码最大长度</summary>
        public const int MaxPasswordLength = 20;

        /// <summary>登录最大重试次数</summary>
        public const int MaxLoginRetryCount = 5;

        /// <summary>锁定时间(分钟)</summary>
        public const int LockoutMinutes = 30;

        /// <summary>Session超时时间(小时)</summary>
        public const int SessionTimeoutHours = 8;

        /// <summary>默认分页大小</summary>
        public const int DefaultPageSize = 15;

        /// <summary>最大分页大小</summary>
        public const int MaxPageSize = 100;

        /// <summary>超级管理员角色编码</summary>
        public const string SuperAdminRoleCode = "super_admin";

        /// <summary>管理员角色编码</summary>
        public const string AdminRoleCode = "admin";
    }
}
