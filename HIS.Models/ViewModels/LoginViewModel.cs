using System.ComponentModel.DataAnnotations;

namespace HIS.Models.ViewModels
{
    /// <summary>
    /// 登录视图模型
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;

        /// <summary>验证码</summary>
        public string? Captcha { get; set; }

        /// <summary>记住我</summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// 修改密码视图模型
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare(nameof(NewPassword), ErrorMessage = "两次密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户列表视图模型
    /// </summary>
    public class SysUserViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int Status { get; set; }
        public string? DepartmentName { get; set; }
        public string? RoleName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 菜单树视图模型
    /// </summary>
    public class MenuTreeViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Icon { get; set; }
        public bool Spread { get; set; }
        public List<MenuTreeViewModel>? Children { get; set; }
    }

    /// <summary>
    /// 侧边栏菜单视图模型
    /// </summary>
    public class SidebarMenuViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string? Icon { get; set; }
        public List<SidebarMenuViewModel>? Child { get; set; }
    }
}
