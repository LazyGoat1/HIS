using HIS.Services.Implementations;
using HIS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HIS.Services
{
    /// <summary>
    /// 服务层依赖注入扩展方法
    /// </summary>
    public static class ServiceServiceExtensions
    {
        /// <summary>
        /// 注册所有业务逻辑层服务
        /// </summary>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // 系统管理
            services.AddScoped<ISysUserService, SysUserService>();
            services.AddScoped<ISysRoleService, SysRoleService>();
            services.AddScoped<ISysMenuService, SysMenuService>();
            services.AddScoped<ISysDepartmentService, SysDepartmentService>();
            services.AddScoped<ISysLogService, SysLogService>();

            // 基础数据
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IDrugService, DrugService>();
            services.AddScoped<IDrugCategoryService, DrugCategoryService>();
            services.AddScoped<IDrugStockService, DrugStockService>();
            services.AddScoped<IChargeService, ChargeService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IChargeItemService, ChargeItemService>();

            // 门诊管理
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IOutpatientService, OutpatientService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();

            // 住院管理
            services.AddScoped<IInpatientService, InpatientService>();
            services.AddScoped<IBedService, BedService>();
            services.AddScoped<IMedicalOrderService, MedicalOrderService>();

            return services;
        }
    }
}
