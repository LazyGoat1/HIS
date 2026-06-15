using HIS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Data
{
    /// <summary>
    /// 医院管理系统数据库上下文
    /// </summary>
    public class HisDbContext : DbContext
    {
        public HisDbContext(DbContextOptions<HisDbContext> options) : base(options)
        {
        }

        // 系统管理
        public DbSet<SysUser> SysUsers => Set<SysUser>();
        public DbSet<SysRole> SysRoles => Set<SysRole>();
        public DbSet<SysMenu> SysMenus => Set<SysMenu>();
        public DbSet<SysRoleMenu> SysRoleMenus => Set<SysRoleMenu>();
        public DbSet<SysDepartment> SysDepartments => Set<SysDepartment>();
        public DbSet<SysLog> SysLogs => Set<SysLog>();

        // 基础数据
        public DbSet<PatientInfo> PatientInfos => Set<PatientInfo>();
        public DbSet<DoctorInfo> DoctorInfos => Set<DoctorInfo>();

        // 门诊
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<OutpatientRecord> OutpatientRecords => Set<OutpatientRecord>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PrescriptionDetail> PrescriptionDetails => Set<PrescriptionDetail>();

        // 住院
        public DbSet<InpatientRecord> InpatientRecords => Set<InpatientRecord>();
        public DbSet<BedInfo> BedInfos => Set<BedInfo>();
        public DbSet<MedicalOrder> MedicalOrders => Set<MedicalOrder>();

        // 药品
        public DbSet<DrugInfo> DrugInfos => Set<DrugInfo>();
        public DbSet<DrugCategory> DrugCategories => Set<DrugCategory>();
        public DbSet<DrugStockLog> DrugStockLogs => Set<DrugStockLog>();

        // 收费
        public DbSet<ChargeRecord> ChargeRecords => Set<ChargeRecord>();
        public DbSet<ChargeItem> ChargeItems => Set<ChargeItem>();
        public DbSet<NursingRecord> NursingRecords => Set<NursingRecord>();
        public DbSet<SysConfig> SysConfigs => Set<SysConfig>();
        public DbSet<Schedule> Schedules => Set<Schedule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ====== 全局禁用级联删除 ======
            // 原因: 实体间存在多重FK路径(如 InpatientRecord→SysDepartment 和
            // InpatientRecord→BedInfo→SysDepartment)，SQL Server 会报错
            // "可能会导致循环或多重级联路径"，数据删除由业务层统一处理
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // 应用所有FluentAPI配置
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HisDbContext).Assembly);

            // 全局软删除过滤器
            modelBuilder.Entity<SysUser>().HasQueryFilter(e => !e.IsDeleted);

            // 索引配置
            modelBuilder.Entity<SysUser>(e =>
            {
                e.HasIndex(u => u.UserName).IsUnique();
                e.HasIndex(u => u.Phone);
            });

            modelBuilder.Entity<SysRole>(e =>
            {
                e.HasIndex(r => r.RoleCode).IsUnique();
            });

            modelBuilder.Entity<PatientInfo>(e =>
            {
                e.HasIndex(p => p.PatientNo).IsUnique();
                e.HasIndex(p => p.IdCard);
                e.HasIndex(p => p.Phone);
            });

            modelBuilder.Entity<DoctorInfo>(e =>
            {
                e.HasIndex(d => d.DoctorNo).IsUnique();
                e.HasIndex(d => d.UserId);
            });

            modelBuilder.Entity<Registration>(e =>
            {
                e.HasIndex(r => r.RegistrationNo).IsUnique();
                e.HasIndex(r => r.VisitDate);
                e.HasIndex(r => r.PatientId);
            });

            modelBuilder.Entity<Prescription>(e =>
            {
                e.HasIndex(p => p.PrescriptionNo).IsUnique();
                e.HasIndex(p => p.PatientId);
                e.HasIndex(p => p.DoctorId);
            });

            modelBuilder.Entity<InpatientRecord>(e =>
            {
                e.HasIndex(i => i.InpatientNo).IsUnique();
                e.HasIndex(i => i.PatientId);
            });

            modelBuilder.Entity<DrugInfo>(e =>
            {
                e.HasIndex(d => d.DrugCode).IsUnique();
                e.HasIndex(d => d.DrugName);
            });

            modelBuilder.Entity<ChargeRecord>(e =>
            {
                e.HasIndex(c => c.ChargeNo).IsUnique();
                e.HasIndex(c => c.PatientId);
                e.HasIndex(c => c.CreateTime);
            });

            modelBuilder.Entity<SysLog>(e =>
            {
                e.HasIndex(l => l.UserId);
                e.HasIndex(l => l.CreateTime);
            });
        }

        /// <summary>
        /// 保存时自动设置实体的 CreateTime
        /// </summary>
        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// 异步保存时自动设置实体的 CreateTime
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                            (e.State == EntityState.Added));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entity.CreateTime == default)
                {
                    entity.CreateTime = DateTime.Now;
                }
            }
        }
    }
}
