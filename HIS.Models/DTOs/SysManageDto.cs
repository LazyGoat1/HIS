namespace HIS.Models.DTOs
{
    /// <summary>
    /// 角色DTO
    /// </summary>
    public class SysRoleDto
    {
        public long Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 菜单DTO
    /// </summary>
    public class SysMenuDto
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int MenuType { get; set; }
        public string? MenuUrl { get; set; }
        public string? MenuIcon { get; set; }
        public int SortOrder { get; set; }
        public string? PermissionCode { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 部门/科室DTO
    /// </summary>
    public class SysDepartmentDto
    {
        public long Id { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public long? ParentId { get; set; }
        public int DeptType { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 日志查询DTO
    /// </summary>
    public class SysLogDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
