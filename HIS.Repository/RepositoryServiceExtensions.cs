using HIS.Repository.Implementations;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Uow = HIS.Repository.UnitOfWork.UnitOfWork;

namespace HIS.Repository
{
    /// <summary>
    /// 仓储层依赖注入扩展方法
    /// </summary>
    public static class RepositoryServiceExtensions
    {
        /// <summary>
        /// 注册所有仓储层服务
        /// </summary>
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // 通用仓储（泛型）
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // 系统管理
            services.AddScoped<ISysUserRepository, SysUserRepository>();
            services.AddScoped<ISysRoleRepository, SysRoleRepository>();
            services.AddScoped<ISysMenuRepository, SysMenuRepository>();
            services.AddScoped<ISysDepartmentRepository, SysDepartmentRepository>();

            // 基础数据
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();

            // 门诊
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<IOutpatientRecordRepository, OutpatientRecordRepository>();
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

            // 药房
            services.AddScoped<IDrugRepository, DrugRepository>();
            services.AddScoped<IDrugCategoryRepository, DrugCategoryRepository>();
            services.AddScoped<IDrugStockRepository, DrugStockRepository>();
            services.AddScoped<IChargeRecordRepository, ChargeRecordRepository>();
            services.AddScoped<IChargeItemRepository, ChargeItemRepository>();

            // 住院
            services.AddScoped<IInpatientRecordRepository, InpatientRecordRepository>();
            services.AddScoped<IBedInfoRepository, BedInfoRepository>();
            services.AddScoped<IMedicalOrderRepository, MedicalOrderRepository>();

            // 工作单元
            services.AddScoped<IUnitOfWork, Uow>();

            return services;
        }
    }
}
