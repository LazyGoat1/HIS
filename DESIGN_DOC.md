# 医院管理系统（HIS）设计文档

> **技术栈**: .NET 8.0 MVC + Layui 2.9 + EF Core 8 + SQL Server + NPOI + ECharts 5  
> **文档版本**: v2.0  
> **创建日期**: 2026-06-13  
> **更新日期**: 2026-06-15

---

## 目录

1. [项目概述](#1-项目概述)
2. [系统架构](#2-系统架构)
3. [技术选型](#3-技术选型)
4. [项目分层结构](#4-项目分层结构)
5. [功能模块设计](#5-功能模块设计)
6. [数据库设计](#6-数据库设计)
7. [路由与控制器设计](#7-路由与控制器设计)
8. [前端架构设计](#8-前端架构设计)
9. [权限与安全设计](#9-权限与安全设计)
10. [接口规范](#10-接口规范)
11. [开发计划](#11-开发计划)
12. [附录](#12-附录)

---

## 1. 项目概述

### 1.1 项目背景

医院管理系统（Hospital Information System，简称 HIS）是面向中小型医院和诊所的全功能信息管理平台。系统覆盖门诊、住院、药房、收费、统计报表等核心业务场景，帮助医疗机构实现信息化管理，提高工作效率和服务质量。

### 1.2 系统目标

- **业务全覆盖**: 覆盖医院日常运营的挂号、诊疗、处方、收费、住院、药房等全流程
- **操作高效**: 基于 Layui 的简洁界面，降低医护人员学习成本，提升操作效率
- **数据安全**: 严格的权限分级管理，保障患者隐私数据和医疗数据安全
- **扩展灵活**: 分层架构设计，便于后期功能扩展和第三方系统对接
- **统计决策**: 提供多维度报表统计，辅助管理层进行经营决策

### 1.3 用户角色

| 角色 | 说明 | 主要权限范围 |
|------|------|-------------|
| 超级管理员 | 系统最高权限者 | 系统配置、用户管理、全部数据查看 |
| 管理员 | 医院管理层 | 员工管理、统计报表、基础数据维护 |
| 医生 | 门诊/住院医生 | 诊疗、处方、病历、医嘱 |
| 护士 | 护理人员 | 护理记录、执行医嘱、住院管理 |
| 收费员 | 财务收费人员 | 挂号收费、门诊收费、住院结算 |
| 药师 | 药房工作人员 | 药品管理、处方发药、库存管理 |
| 患者 | 注册用户(可选) | 在线预约、查看报告 |

---

## 2. 系统架构

### 2.1 总体架构

```
┌─────────────────────────────────────────────────────────────┐
│                      客户端 (Browser)                        │
│               Layui + jQuery + ECharts                       │
├─────────────────────────────────────────────────────────────┤
│                    表示层 (HIS.Web)                          │
│            ASP.NET Core MVC (.NET 8.0)                      │
│         Controllers → Views → ViewModels                    │
├─────────────────────────────────────────────────────────────┤
│                   业务逻辑层 (HIS.Services)                    │
│          Service Interfaces → Service Implementations        │
│            DTO 转换 / 业务验证 / 工作流控制                    │
├─────────────────────────────────────────────────────────────┤
│                    数据访问层 (HIS.Repository)                │
│          Repository Interfaces → Repository Implementations  │
│              Entity Framework Core / Dapper                  │
├─────────────────────────────────────────────────────────────┤
│                     数据层 (Database)                        │
│                      SQL Server 2019+                        │
└─────────────────────────────────────────────────────────────┘
                           │
           ┌───────────────┴───────────────┐
           │                               │
     HIS.Models                      HIS.Common
   (实体/视图模型/DTO)            (工具类/常量/枚举/扩展)
```

### 2.2 依赖关系

```
HIS.Web → HIS.Services → HIS.Repository → HIS.Models
    ↓            ↓              ↓
    └────────────┴──────────────┴──────→ HIS.Common
```

- **HIS.Web** 引用 HIS.Services、HIS.Models、HIS.Common
- **HIS.Services** 引用 HIS.Repository、HIS.Models、HIS.Common
- **HIS.Repository** 引用 HIS.Models、HIS.Common
- **HIS.Models** 引用 HIS.Common
- **HIS.Common** 无项目依赖（基础设施层）

### 2.3 请求处理流程

```
用户请求 → Controller → Service(业务逻辑) → Repository(数据操作) → DB
                ↓                                    ↓
           ViewModel/DTO                          Entity
                ↓
           View(HTML) → 响应返回浏览器
```

---

## 3. 技术选型

| 层级 | 技术 | 版本 | 说明 |
|------|------|------|------|
| 运行时 | .NET | 8.0 | 最新 LTS 版本 |
| Web框架 | ASP.NET Core MVC | 8.0 | 服务端渲染 + API |
| 前端UI | Layui | 2.9.x | 经典模块化前端 UI 框架 |
| 数据操作 | Entity Framework Core | 8.0 | ORM，支持 Code First |
| 数据库 | SQL Server | 2019+ | 关系型数据库 |
| 缓存 | MemoryCache / Redis | - | 本地缓存 + 分布式缓存(可选) |
| 日志 | Serilog | 4.x | 结构化日志记录 |
| 对象映射 | AutoMapper | 13.x | Entity ↔ DTO 自动映射 |
| 认证授权 | ASP.NET Core Identity | - | 内置身份认证框架 |
| 图表 | ECharts | 5.x | 报表统计可视化 |
| Excel | NPOI | 2.7 | Excel 导入/导出 |
| 图标 | Layui Icon | - | 图标库 |
| IOC容器 | Microsoft.Extensions.DI | - | .NET 内置依赖注入 |
| 存储过程 | T-SQL | - | 5个核心 SP：日收入/医生工作量/库存预警/科室统计/处方发药汇总 |

### 3.1 NuGet 包清单

```xml
<!-- HIS.Web -->
- Microsoft.EntityFrameworkCore.SqlServer (8.0.x)
- Microsoft.EntityFrameworkCore.Tools (8.0.x)
- AutoMapper.Extensions.Microsoft.DependencyInjection (12.x)
- Serilog.AspNetCore (8.x)

<!-- HIS.Services -->
- AutoMapper (13.x)

<!-- HIS.Repository -->
- Microsoft.EntityFrameworkCore (8.0.x)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.x)

<!-- HIS.Common -->
- 无外部依赖（纯工具库）
```

---

## 4. 项目分层结构

### 4.1 目录结构总览

```
HospitalSysProject/
├── HospitalSysProject.sln
├── DESIGN_DOC.md                    # 本设计文档
│
├── HIS.Common/                      # 公共基础设施层
│   ├── HIS.Common.csproj
│   ├── Constants/                   # 常量定义
│   │   ├── SystemConstants.cs       # 系统常量
│   │   └── CacheKeys.cs             # 缓存键常量
│   ├── Enums/                       # 枚举定义
│   │   ├── GenderEnum.cs
│   │   ├── PaymentStatusEnum.cs
│   │   ├── RegistrationStatusEnum.cs
│   │   ├── PrescriptionStatusEnum.cs
│   │   └── ...
│   ├── Extensions/                  # 扩展方法
│   │   ├── StringExtensions.cs
│   │   ├── DateTimeExtensions.cs
│   │   └── EnumExtensions.cs
│   ├── Helpers/                     # 工具类
│   │   ├── EncryptionHelper.cs     # 加密工具
│   │   ├── GenerateNoHelper.cs     # 编号生成(挂号单号/处方号等)
│   │   └── PinyinHelper.cs         # 拼音辅助
│   └── Attributes/                  # 自定义特性
│       └── ExcelColumnAttribute.cs  # Excel导出特性
│
├── HIS.Models/                      # 模型层
│   ├── HIS.Models.csproj
│   ├── Entities/                    # 数据库实体
│   │   ├── SysUser.cs
│   │   ├── SysRole.cs
│   │   ├── SysMenu.cs
│   │   ├── SysDepartment.cs
│   │   ├── PatientInfo.cs
│   │   ├── DoctorInfo.cs
│   │   ├── Registration.cs
│   │   ├── OutpatientRecord.cs
│   │   ├── Prescription.cs
│   │   ├── PrescriptionDetail.cs
│   │   ├── DrugInfo.cs
│   │   ├── InpatientRecord.cs
│   │   ├── BedInfo.cs
│   │   ├── MedicalOrder.cs
│   │   ├── ChargeRecord.cs
│   │   └── ...
│   ├── ViewModels/                  # 视图模型(页面专用)
│   │   ├── LoginViewModel.cs
│   │   ├── RegistrationViewModel.cs
│   │   ├── PrescriptionViewModel.cs
│   │   └── ...
│   ├── DTOs/                        # 数据传输对象(层间交互)
│   │   ├── PatientDto.cs
│   │   ├── DoctorDto.cs
│   │   └── ...
│   └── QueryModels/                 # 查询条件模型
│       ├── PatientQueryModel.cs
│       └── ...
│
├── HIS.Repository/                  # 数据访问层
│   ├── HIS.Repository.csproj
│   ├── Data/
│   │   └── HisDbContext.cs          # EF Core 数据库上下文
│   ├── Configurations/              # Entity 配置 (Fluent API)
│   │   ├── SysUserConfiguration.cs
│   │   ├── PatientInfoConfiguration.cs
│   │   └── ...
│   ├── Interfaces/                  # 仓储接口
│   │   ├── IBaseRepository.cs       # 基础仓储接口
│   │   ├── ISysUserRepository.cs
│   │   ├── IPatientRepository.cs
│   │   ├── IRegistrationRepository.cs
│   │   └── ...
│   ├── Implementations/             # 仓储实现
│   │   ├── BaseRepository.cs        # 基础仓储实现
│   │   ├── SysUserRepository.cs
│   │   ├── PatientRepository.cs
│   │   └── ...
│   └── UnitOfWork/
│       ├── IUnitOfWork.cs
│       └── UnitOfWork.cs
│
├── HIS.Services/                    # 业务逻辑层
│   ├── HIS.Services.csproj
│   ├── Interfaces/                  # 服务接口
│   │   ├── ISysUserService.cs
│   │   ├── IPatientService.cs
│   │   ├── IRegistrationService.cs
│   │   ├── IOutpatientService.cs
│   │   ├── IPrescriptionService.cs
│   │   ├── IDrugService.cs
│   │   ├── IInpatientService.cs
│   │   ├── IChargeService.cs
│   │   ├── IStatisticsService.cs
│   │   └── ...
│   └── Implementations/             # 服务实现
│       ├── SysUserService.cs
│       ├── PatientService.cs
│       ├── RegistrationService.cs
│       └── ...
│
└── HIS.Web/                         # 表现层 (MVC Web)
    ├── HIS.Web.csproj
    ├── Program.cs                   # 应用启动入口
    ├── Controllers/                 # 控制器
    │   ├── HomeController.cs
    │   ├── AccountController.cs     # 登录/登出/个人信息
    │   ├── SysUserController.cs     # 用户管理
    │   ├── SysRoleController.cs     # 角色管理
    │   ├── PatientController.cs     # 患者管理
    │   ├── RegistrationController.cs # 挂号管理
    │   ├── OutpatientController.cs  # 门诊管理
    │   ├── PrescriptionController.cs # 处方管理
    │   ├── DrugController.cs        # 药品管理
    │   ├── InpatientController.cs   # 住院管理
    │   ├── ChargeController.cs      # 收费管理
    │   ├── StatisticsController.cs  # 统计报表
    │   └── ...
    ├── Views/                       # 视图页面
    │   ├── _ViewStart.cshtml
    │   ├── _ViewImports.cshtml
    │   ├── Shared/
    │   │   ├── _Layout.cshtml       # 主布局(集成Layui)
    │   │   ├── _LayoutLogin.cshtml  # 登录页布局
    │   │   ├── _Sidebar.cshtml      # 侧边栏菜单
    │   │   └── _Pagination.cshtml   # 分页组件
    │   ├── Home/
    │   │   └── Index.cshtml         # 首页仪表盘
    │   ├── Account/
    │   │   ├── Login.cshtml
    │   │   └── Profile.cshtml
    │   ├── SysUser/
    │   │   ├── Index.cshtml         # 用户列表
    │   │   ├── Create.cshtml        # 新增用户
    │   │   └── Edit.cshtml          # 编辑用户
    │   ├── Patient/
    │   ├── Registration/
    │   ├── Outpatient/
    │   ├── Prescription/
    │   ├── Drug/
    │   ├── Inpatient/
    │   ├── Charge/
    │   └── Statistics/
    ├── wwwroot/                     # 静态资源
    │   ├── layui/                   # Layui 框架文件
    │   ├── js/
    │   │   ├── common.js            # 公共 JS
    │   │   ├── layui-ext/           # Layui 扩展模块
    │   │   └── pages/               # 各页面 JS
    │   ├── css/
    │   │   ├── common.css           # 公共样式
    │   │   └── login.css            # 登录页样式
    │   └── images/                  # 图片资源
    └── Filters/                     # 过滤器
        ├── AuthFilter.cs            # 登录验证过滤器
        └── PermissionFilter.cs      # 权限验证过滤器
```

---

## 5. 功能模块设计

### 5.1 模块全景

```
医院管理系统 (HIS)
│
├── 1. 系统管理 (System)
│   ├── 1.1 用户管理
│   ├── 1.2 角色管理
│   ├── 1.3 菜单管理
│   ├── 1.4 部门管理
│   ├── 1.5 操作日志
│   └── 1.6 系统配置
│
├── 2. 基础数据 (BasicData)
│   ├── 2.1 患者信息管理
│   ├── 2.2 医生信息管理
│   ├── 2.3 科室管理
│   ├── 2.4 药品字典管理
│   └── 2.5 收费项目管理
│
├── 3. 门诊管理 (Outpatient)
│   ├── 3.1 挂号管理
│   ├── 3.2 门诊诊疗
│   ├── 3.3 处方管理
│   ├── 3.4 门诊收费
│   └── 3.5 门诊退费
│
├── 4. 住院管理 (Inpatient)
│   ├── 4.1 入院登记
│   ├── 4.2 床位管理
│   ├── 4.3 医嘱管理
│   ├── 4.4 护理记录
│   ├── 4.5 住院收费
│   └── 4.6 出院结算
│
├── 5. 药房管理 (Pharmacy)
│   ├── 5.1 药品入库 / 出库
│   ├── 5.2 库存管理（流水查询）
│   ├── 5.3 处方发药 / 退药
│   ├── 5.4 库存盘点
│   └── 5.5 库存预警（首页 + 库存页）
│
├── 6. 收费管理 (Finance)
│   ├── 6.1 收费流水（多条件搜索 + 导出）
│   ├── 6.2 门诊收费（关联处方自动填金额）
│   ├── 6.3 退费管理
│   └── 6.4 挂号费自动生成收费记录
│
├── 7. 统计报表 (Statistics)
│   ├── 7.1 实时仪表盘（ECharts：今日概览/门诊趋势/收入趋势/科室排名/药品排名）
│   ├── 7.2 首页 30 秒自动刷新
│   └── 7.3 日结报表 API
│
├── 8. 排班管理 (Schedule)
│   ├── 8.1 医生排班 CRUD（按星期/时段/限号）
│   ├── 8.2 挂号联动：只显示今日有排班的医生
│   └── 8.3 防重复排班校验
│
├── 9. 护理记录 (Nursing)
│   └── 9.1 关联住院患者 + 床位显示
│
└── 10. 系统配置 (SysConfig)
    └── 10.1 键值对配置管理
```

### 5.2 核心业务流程

#### 5.2.1 门诊流程

```
患者到院 → 挂号(选择科室/医生) → 分诊 → 医生接诊
    → 问诊/检查 → 开处方 → 收费处缴费 → 药房取药
    → (需要住院则转入院流程)
```

#### 5.2.2 住院流程

```
入院登记 → 分配床位 → 医生下达医嘱 → 护士执行医嘱
    → 每日费用记账 → 出院结算 → 出院
```

#### 5.2.3 处方流程

```
医生开方 → 处方生成 → 收费确认 → 药房审核 → 配药发药
```

---

## 6. 数据库设计

### 6.1 核心表结构

#### 6.1.1 系统管理相关表

**SysUser（系统用户表）**

| 字段名 | 类型 | 长度 | 可空 | 说明 |
|--------|------|------|------|------|
| Id | bigint | - | N | 主键，自增 |
| UserName | nvarchar | 50 | N | 用户名(登录账号) |
| Password | nvarchar | 256 | N | 密码(Hash存储) |
| RealName | nvarchar | 20 | N | 真实姓名 |
| Gender | int | - | N | 性别 0:女 1:男 |
| Phone | nvarchar | 20 | Y | 手机号 |
| Email | nvarchar | 100 | Y | 邮箱 |
| Avatar | nvarchar | 500 | Y | 头像URL |
| DepartmentId | bigint | - | Y | 所属科室ID |
| RoleId | bigint | - | N | 角色ID |
| Status | int | - | N | 状态 0:禁用 1:启用 |
| LastLoginTime | datetime | - | Y | 最后登录时间 |
| CreateTime | datetime | - | N | 创建时间 |
| IsDeleted | bit | - | N | 软删除标记 |

**SysRole（角色表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| RoleName | nvarchar(50) | 角色名称 |
| RoleCode | nvarchar(50) | 角色编码 |
| Description | nvarchar(200) | 描述 |
| SortOrder | int | 排序 |
| Status | int | 状态 |
| CreateTime | datetime | 创建时间 |

**SysMenu（菜单表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| ParentId | bigint | 父菜单ID |
| MenuName | nvarchar(50) | 菜单名称 |
| MenuType | int | 类型 1:目录 2:菜单 3:按钮 |
| MenuUrl | nvarchar(200) | 菜单URL/路由 |
| MenuIcon | nvarchar(50) | 图标 |
| SortOrder | int | 排序 |
| PermissionCode | nvarchar(100) | 权限标识码 |
| Status | int | 状态 |
| CreateTime | datetime | 创建时间 |

**SysRoleMenu（角色菜单关联表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| RoleId | bigint | 角色ID |
| MenuId | bigint | 菜单ID |

**SysDepartment（科室表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| DeptName | nvarchar(50) | 科室名称 |
| DeptCode | nvarchar(20) | 科室编码 |
| ParentId | bigint | 上级科室ID(支持层级) |
| DeptType | int | 类型 1:临床 2:医技 3:行政 |
| Phone | nvarchar(20) | 联系电话 |
| Location | nvarchar(100) | 位置 |
| Description | nvarchar(200) | 描述 |
| SortOrder | int | 排序 |
| Status | int | 状态 |
| CreateTime | datetime | 创建时间 |

**SysLog（操作日志表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| UserId | bigint | 操作用户ID |
| UserName | nvarchar(50) | 操作用户名 |
| Module | nvarchar(50) | 操作模块 |
| Action | nvarchar(50) | 操作类型 |
| Description | nvarchar(500) | 操作描述 |
| IPAddress | nvarchar(50) | IP地址 |
| CreateTime | datetime | 操作时间 |

---

#### 6.1.2 基础数据相关表

**PatientInfo（患者信息表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| PatientNo | nvarchar(50) | 患者编号(唯一) |
| Name | nvarchar(20) | 姓名 |
| Gender | int | 性别 |
| Birthday | date | 出生日期 |
| Age | int | 年龄(计算字段) |
| IdCard | nvarchar(18) | 身份证号 |
| Phone | nvarchar(20) | 手机号 |
| Address | nvarchar(200) | 地址 |
| BloodType | nvarchar(10) | 血型 |
| AllergyHistory | nvarchar(500) | 过敏史 |
| CreateTime | datetime | 创建时间 |

**DoctorInfo（医生信息表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| UserId | bigint | 关联系统用户ID |
| DoctorNo | nvarchar(50) | 医生工号 |
| Name | nvarchar(20) | 姓名 |
| Gender | int | 性别 |
| DepartmentId | bigint | 科室ID |
| Title | int | 职称 1:主任医师 2:副主任医师 3:主治医师 4:住院医师 |
| Specialty | nvarchar(200) | 专长 |
| MaxDailyPatients | int | 每日最大接诊数 |
| ConsultationFee | decimal(18,2) | 挂号费/诊疗费 |
| Status | int | 状态 0:休假 1:在岗 |
| CreateTime | datetime | 创建时间 |

---

#### 6.1.3 门诊相关表

**Registration（挂号表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| RegistrationNo | nvarchar(50) | 挂号单号 |
| PatientId | bigint | 患者ID |
| DepartmentId | bigint | 科室ID |
| DoctorId | bigint | 医生ID |
| RegistrationType | int | 类型 1:普通 2:专家 3:急诊 |
| RegistrationFee | decimal(18,2) | 挂号费 |
| Status | int | 状态 1:已挂号 2:已接诊 3:已退号 |
| VisitDate | date | 就诊日期 |
| QueueNumber | int | 排队序号 |
| CreateTime | datetime | 挂号时间 |
| CreateUserId | bigint | 挂号操作员ID |

**OutpatientRecord（门诊记录表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| RegistrationId | bigint | 挂号ID |
| PatientId | bigint | 患者ID |
| DoctorId | bigint | 医生ID |
| ChiefComplaint | nvarchar(500) | 主诉 |
| PresentIllness | nvarchar(1000) | 现病史 |
| PastHistory | nvarchar(500) | 既往史 |
| PhysicalExamination | nvarchar(1000) | 体格检查 |
| PreliminaryDiagnosis | nvarchar(500) | 初步诊断 |
| Advice | nvarchar(1000) | 医嘱建议 |
| VisitTime | datetime | 就诊时间 |

**Prescription（处方表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| PrescriptionNo | nvarchar(50) | 处方号 |
| RegistrationId | bigint | 挂号ID |
| OutpatientRecordId | bigint | 门诊记录ID |
| PatientId | bigint | 患者ID |
| DoctorId | bigint | 医生ID |
| PrescriptionType | int | 类型 1:西药 2:中药 3:检查 |
| TotalAmount | decimal(18,2) | 合计金额 |
| Status | int | 状态 1:已开具 2:已收费 3:已发药 4:已退方 |
| Remark | nvarchar(500) | 备注 |
| CreateTime | datetime | 开具时间 |

**PrescriptionDetail（处方明细表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| PrescriptionId | bigint | 处方ID |
| ItemType | int | 项目类型 1:药品 2:检查项目 |
| ItemId | bigint | 项目ID(药品ID或检查项目ID) |
| ItemName | nvarchar(100) | 项目名称 |
| Specification | nvarchar(100) | 规格 |
| Unit | nvarchar(20) | 单位 |
| UnitPrice | decimal(18,4) | 单价 |
| Quantity | int | 数量 |
| Amount | decimal(18,2) | 金额 |
| Usage | nvarchar(200) | 用法 |
| Dosage | nvarchar(100) | 用量 |
| Frequency | nvarchar(50) | 频次 |
| Days | int | 天数 |

---

#### 6.1.4 住院相关表

**InpatientRecord（住院记录表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| InpatientNo | nvarchar(50) | 住院号 |
| PatientId | bigint | 患者ID |
| BedId | bigint | 床位ID |
| DepartmentId | bigint | 科室ID |
| DoctorId | bigint | 主治医生ID |
| AdmissionTime | datetime | 入院时间 |
| DischargeTime | datetime | 出院时间 |
| AdmissionDiagnosis | nvarchar(500) | 入院诊断 |
| DischargeDiagnosis | nvarchar(500) | 出院诊断 |
| Status | int | 状态 1:在院 2:出院 3:转科 |
| DepositAmount | decimal(18,2) | 预交金 |
| TotalCost | decimal(18,2) | 总费用 |
| CreateTime | datetime | 创建时间 |

**BedInfo（床位表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| BedNo | nvarchar(20) | 床位号 |
| RoomNo | nvarchar(20) | 房间号 |
| DepartmentId | bigint | 所属科室 |
| BedType | int | 类型 1:普通 2:双人 3:单人 4:VIP |
| DailyRate | decimal(18,2) | 日床位费 |
| Status | int | 状态 1:空闲 2:占用 3:维修 |
| CreateTime | datetime | 创建时间 |

**MedicalOrder（医嘱表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| InpatientId | bigint | 住院ID |
| PatientId | bigint | 患者ID |
| DoctorId | bigint | 医生ID |
| OrderType | int | 类型 1:长期 2:临时 |
| OrderContent | nvarchar(500) | 医嘱内容 |
| StartTime | datetime | 开始时间 |
| EndTime | datetime | 结束时间 |
| Status | int | 状态 1:已下达 2:执行中 3:已完成 4:已停止 |
| CreateTime | datetime | 创建时间 |

---

#### 6.1.5 药品相关表

**DrugInfo（药品信息表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| DrugCode | nvarchar(50) | 药品编码 |
| DrugName | nvarchar(100) | 药品名称 |
| GenericName | nvarchar(100) | 通用名 |
| CategoryId | bigint | 分类ID |
| Specification | nvarchar(100) | 规格 |
| Unit | nvarchar(20) | 单位 |
| Manufacturer | nvarchar(200) | 生产厂家 |
| UnitPrice | decimal(18,4) | 单价 |
| RetailPrice | decimal(18,4) | 零售价 |
| StockQuantity | int | 库存数量 |
| MinStock | int | 最低库存预警 |
| IsPrescription | bit | 是否处方药 |
| Status | int | 状态 |
| CreateTime | datetime | 创建时间 |

**DrugCategory（药品分类表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| CategoryName | nvarchar(50) | 分类名称 |
| ParentId | bigint | 上级分类 |
| SortOrder | int | 排序 |

**DrugStockLog（药品库存日志表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| DrugId | bigint | 药品ID |
| ChangeType | int | 类型 1:入库 2:出库 3:发药 4:退药 5:盘点 |
| ChangeQuantity | int | 变更数量(+/-) |
| BeforeQuantity | int | 变更前数量 |
| AfterQuantity | int | 变更后数量 |
| RelatedNo | nvarchar(50) | 关联单号 |
| CreateTime | datetime | 操作时间 |
| CreateUserId | bigint | 操作人 |

---

#### 6.1.6 收费相关表

**ChargeRecord（收费记录表）**

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | bigint | 主键 |
| ChargeNo | nvarchar(50) | 收费单号 |
| PatientId | bigint | 患者ID |
| ChargeType | int | 类型 1:挂号 2:门诊 3:住院预交 4:住院结算 |
| RelatedId | bigint | 关联业务ID(挂号ID/处方ID/住院ID) |
| TotalAmount | decimal(18,2) | 应收金额 |
| PaidAmount | decimal(18,2) | 实收金额 |
| PaymentMethod | int | 支付方式 1:现金 2:微信 3:支付宝 4:银行卡 5:医保 |
| Status | int | 状态 1:已收费 2:已退费 |
| CreateTime | datetime | 收费时间 |
| CreateUserId | bigint | 收费员ID |

---

### 6.2 数据库关系图

```
SysRole ──┐                 SysDepartment
           │                      │
    SysRoleMenu                  │
           │                      │
    ┌── SysMenu              DoctorInfo ──── SysUser
    │                              │
    │                         Registration ──── PatientInfo
    │                              │
    │                         OutpatientRecord
    │                              │
    │                         Prescription ──── PrescriptionDetail
    │                              │                   │
    │                         ChargeRecord          DrugInfo ─── DrugCategory
    │                                                 │
    │                                          DrugStockLog
    │
    │    InpatientRecord ──── BedInfo
    │         │
    │    MedicalOrder
    │
    └──────────────────────────────┘
```

---

## 6.3 存储过程（Stored Procedures）

在 `HIS.Repository/SQL/StoredProcedures.sql` 中定义，共 5 个：

| 存储过程 | 功能 | 参数 |
|----------|------|------|
| `sp_GetDailyRevenueReport` | 日收入报表（按收费类型+支付方式汇总） | `@ReportDate` |
| `sp_GetDoctorWorkload` | 医生工作量统计（挂号/接诊/处方数） | `@StartDate, @EndDate` |
| `sp_GetDrugStockAlert` | 库存预警（缺货/紧急/不足分级） | 无 |
| `sp_GetDepartmentVisitStats` | 科室就诊统计（指定月份） | `@YearMonth` |
| `sp_GetPrescriptionDrugSummary` | 处方发药汇总（按药品统计用量） | `@StartDate, @EndDate` |

> **为何使用存储过程**：复杂聚合查询（多表 JOIN + GROUP BY + 日期范围）放在数据库端执行，减少网络传输和内存开销。SQL Server 的查询优化器可以对存储过程生成更高效的执行计划。

---

## 7. 路由与控制器设计

### 7.1 路由规范

采用 ASP.NET Core MVC 的 **Area（区域）** 功能，按大模块划分 Area，每个 Area 下按子模块组织 Controller。

| Area | Controller | 路由 | 说明 |
|------|-----------|------|------|
| (无) | Account | /Account/Login | 登录/登出 |
| (无) | Home | /Home/Index | 首页仪表盘 |
| System | SysUser | /System/SysUser/Index | 用户管理 |
| System | SysRole | /System/SysRole/Index | 角色管理 |
| System | SysMenu | /System/SysMenu/Index | 菜单管理 |
| System | SysDepartment | /System/SysDepartment/Index | 部门管理 |
| System | SysLog | /System/SysLog/Index | 操作日志 |
| BasicData | Patient | /BasicData/Patient/Index | 患者管理 |
| BasicData | Doctor | /BasicData/Doctor/Index | 医生管理 |
| Outpatient | Registration | /Outpatient/Registration/Index | 挂号管理 |
| Outpatient | OutpatientRecord | /Outpatient/OutpatientRecord/Index | 门诊诊疗 |
| Outpatient | Prescription | /Outpatient/Prescription/Index | 处方管理 |
| Inpatient | InpatientRecord | /Inpatient/InpatientRecord/Index | 住院管理 |
| Inpatient | BedInfo | /Inpatient/BedInfo/Index | 床位管理 |
| Inpatient | MedicalOrder | /Inpatient/MedicalOrder/Index | 医嘱管理 |
| Pharmacy | DrugInfo | /Pharmacy/DrugInfo/Index | 药品管理 |
| Pharmacy | DrugStock | /Pharmacy/DrugStock/Index | 库存管理 |
| Finance | Charge | /Finance/Charge/Index | 收费管理 |
| Statistics | Report | /Statistics/Report/Outpatient | 门诊统计 |
| Statistics | Report | /Statistics/Report/Income | 收入统计 |

### 7.2 Controller 设计规范

每个 Controller 遵循标准 CRUD + 列表分页的 Action 模式：

```csharp
// 以 PatientController 为例
public class PatientController : Controller
{
    // GET: 列表页
    public IActionResult Index() { ... }

    // GET: API 分页查询 (供 Layui Table 调用)
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PatientQueryModel query) { ... }

    // GET: 新增页面
    public IActionResult Create() { ... }

    // POST: 新增数据
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientDto dto) { ... }

    // GET: 编辑页面
    public async Task<IActionResult> Edit(long id) { ... }

    // POST: 更新数据
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] PatientDto dto) { ... }

    // POST: 删除
    [HttpPost]
    public async Task<IActionResult> Delete(long id) { ... }

    // GET: 详情页
    public async Task<IActionResult> Detail(long id) { ... }
}
```

---

## 8. 前端架构设计

### 8.1 Layui 集成方案

基于 Layui 经典版构建管理后台 UI，采用以下布局结构：

```
┌─────────────────────────────────────────────────────────────┐
│  Header（顶部导航栏）                                         │
│  Logo │ 系统名称 │ 快捷操作 │ 用户信息 │ 退出                  │
├──────────┬──────────────────────────────────────────────────┤
│          │                                                  │
│ Sidebar  │             Body（内容区域）                      │
│ 侧边栏   │                                                  │
│ 菜单导航  │    ┌─────────────────────────────────────┐      │
│          │    │  面包屑导航                           │      │
│          │    ├─────────────────────────────────────┤      │
│          │    │                                     │      │
│          │    │  页面内容区                           │      │
│          │    │  (表格/表单/详情)                     │      │
│          │    │                                     │      │
│          │    └─────────────────────────────────────┘      │
│          │                                                  │
├──────────┴──────────────────────────────────────────────────┤
│  Footer（底部）  © 2026 医院管理系统                          │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 _Layout.cshtml 改造

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>@ViewData["Title"] - 医院管理系统</title>
    <link rel="stylesheet" href="~/layui/css/layui.css" />
    <link rel="stylesheet" href="~/css/common.css" />
    @RenderSection("Styles", required: false)
</head>
<body>
    <div class="layui-layout layui-layout-admin">
        <!-- 顶部 -->
        <div class="layui-header">
            <div class="layui-logo">医院管理系统</div>
            <ul class="layui-nav layui-layout-right">
                <li class="layui-nav-item">
                    <a href="javascript:;"><img src="@avatar" class="layui-nav-img" />@UserName</a>
                    <dl class="layui-nav-child">
                        <dd><a href="/Account/Profile">个人信息</a></dd>
                        <dd><a href="/Account/ChangePassword">修改密码</a></dd>
                        <dd><a href="/Account/Logout">退出登录</a></dd>
                    </dl>
                </li>
            </ul>
        </div>

        <!-- 侧边栏 -->
        <div class="layui-side layui-bg-black">
            <div class="layui-side-scroll">
                <ul id="sidebarMenu" class="layui-nav layui-nav-tree">
                    <!-- JS 动态渲染菜单 -->
                </ul>
            </div>
        </div>

        <!-- 内容区 -->
        <div class="layui-body">
            <span class="layui-breadcrumb" style="padding:10px;">
                <a href="/">首页</a>
                <a><cite>@ViewData["Module"]</cite></a>
            </span>
            <div style="padding:15px;">
                @RenderBody()
            </div>
        </div>

        <!-- 底部 -->
        <div class="layui-footer">
            © 2026 医院管理系统
        </div>
    </div>

    <script src="~/layui/layui.js"></script>
    <script src="~/js/common.js"></script>
    @RenderSection("Scripts", required: false)
</body>
</html>
```

### 8.3 Layui 组件使用规范

| 组件 | 使用场景 |
|------|---------|
| `table` | 所有数据列表展示，支持分页、排序、搜索 |
| `form` | 新增/编辑操作 |
| `tree` | 菜单管理、药品分类、科室层级 |
| `treeTable` | 有层级结构的列表 |
| `laydate` | 日期选择（就诊日期、报表日期范围等） |
| `upload` | 头像上传、文件导入 |
| `dropdown` | 顶部用户菜单、行操作菜单 |
| `element` | Tab切换、面板折叠 |
| `layer` | 弹窗提示、确认框、iframe页面 |
| `flow` | 滚动加载（适合长列表） |

### 8.4 前端请求封装

```javascript
// common.js - 封装 Layui Table 配置和 Ajax 请求

// 通用表格渲染
function renderTable(tableId, url, cols, where = {}) {
    layui.table.render({
        elem: '#' + tableId,
        url: url,
        method: 'get',
        where: where,
        page: true,
        limit: 15,
        limits: [10, 15, 20, 50],
        cols: [cols],
        request: { pageName: 'pageIndex', limitName: 'pageSize' },
        response: { statusCode: 0, countName: 'total', dataName: 'data' }
    });
}

// 通用 Ajax 封装
var http = {
    get: function(url, data) {
        return $.ajax({ url: url, type: 'GET', data: data });
    },
    post: function(url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        });
    }
};

// 统一响应处理
function handleResponse(res, successCallback) {
    if (res.code === 0) {
        layui.layer.msg(res.msg || '操作成功', { icon: 1 });
        if (successCallback) successCallback(res);
    } else {
        layui.layer.msg(res.msg || '操作失败', { icon: 2 });
    }
}
```

### 8.5 统一 API 响应格式

```csharp
// 所有 Ajax 接口返回统一格式
public class ApiResult
{
    public int Code { get; set; }        // 0:成功 1:失败
    public string Msg { get; set; }      // 提示信息
    public object Data { get; set; }     // 返回数据
    public int Count { get; set; }       // 数据总数(分页)
}

// 专用于 Layui Table 的响应
public class LayuiTableResult
{
    public int Code { get; set; }        // 0
    public string Msg { get; set; }      // ""
    public int Total { get; set; }       // 总记录数
    public object Data { get; set; }     // 数据列表
}
```

---

## 9. 权限与安全设计

### 9.1 认证方案

采用 **ASP.NET Core Cookie Authentication**：

```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
```

### 9.2 授权方案

基于 **角色 + 菜单权限** 的 RBAC 模型：

```
用户(User) → 角色(Role) → 菜单权限(Menu + Button Permission)
```

- **菜单权限**: 控制用户可见的菜单项
- **按钮权限**: 控制页面内的增/删/改/查/导出等操作按钮
- **数据权限**: (可选) 科室级别数据隔离

### 9.3 密码安全

- 使用 **PBKDF2 / BCrypt** 进行密码哈希存储
- 密码复杂度要求：最小8位，包含字母+数字
- 连续登录失败5次锁定账号30分钟

### 9.4 安全措施清单

- [x] CSRF 防护 (ASP.NET Core 内置 AntiforgeryToken)
- [x] XSS 防护 (Razor 自动 HTML 编码)
- [x] SQL 注入防护 (EF Core 参数化查询)
- [x] 敏感数据加密存储
- [x] 操作日志全量记录
- [x] Session 超时自动退出

---

## 10. 接口规范

### 10.1 接口命名规范

| 操作 | HTTP Method | URL Pattern | 示例 |
|------|-------------|-------------|------|
| 列表页 | GET | /{Area}/{Controller}/Index | /BasicData/Patient/Index |
| 分页查询 | GET | /{Area}/{Controller}/GetList | /BasicData/Patient/GetList |
| 新增页 | GET | /{Area}/{Controller}/Create | /BasicData/Patient/Create |
| 新增保存 | POST | /{Area}/{Controller}/Create | /BasicData/Patient/Create |
| 编辑页 | GET | /{Area}/{Controller}/Edit/{id} | /BasicData/Patient/Edit/1 |
| 编辑保存 | POST | /{Area}/{Controller}/Edit | /BasicData/Patient/Edit |
| 删除 | POST | /{Area}/{Controller}/Delete | /BasicData/Patient/Delete |
| 详情 | GET | /{Area}/{Controller}/Detail/{id} | /BasicData/Patient/Detail/1 |
| 导出 | GET | /{Area}/{Controller}/Export | /BasicData/Patient/Export |

### 10.2 代码规范

- **命名**: Controller: PascalCase; Action: PascalCase; 私有字段: _camelCase
- **注释**: 公共方法使用 XML 文档注释
- **异步**: 所有数据库操作使用 async/await
- **异常**: Service 层统一处理，Controller 不捕获异常(由全局过滤器处理)

---

## 11. 开发计划

### 11.1 阶段划分

| 阶段 | 内容 | 预计工期 |
|------|------|---------|
| **第一阶段: 基础设施搭建** | 数据库设计建表、EF Core 配置、依赖注入、AutoMapper 配置、基础仓储/服务框架、登录认证、主布局(集成Layui)、菜单渲染 | 3-5天 |
| **第二阶段: 系统管理** | 用户管理、角色管理、菜单管理、权限分配、操作日志 | 5-7天 |
| **第三阶段: 基础数据** | 患者管理、医生管理、科室管理、药品字典、收费项目管理 | 3-5天 |
| **第四阶段: 门诊管理** | 🟢 已完成 | 挂号、门诊诊疗、处方管理、门诊收费、排班管理 |
| **第五阶段: 住院管理** | 🟢 已完成 | 入院登记、床位管理、医嘱管理、护理记录、出院结算 |
| **第六阶段: 药房管理** | 🟢 已完成 | 药品入库、出库、库存管理、处方发药、盘点、预警 |
| **第七阶段: 收费管理** | 🟢 已完成 | 收费流水、门诊收费、退费、Excel 导出 |
| **第八阶段: 统计报表** | 🟢 已完成 | ECharts 仪表盘、日结、库存预警、首页实时刷新 |
| **第九阶段: 增强功能** | 🟢 已完成 | 存储过程、Excel 导入/导出、个人信息/头像、多选导出、中文表头 |

### 11.2 开发顺序建议

```
Phase 1: 基础设施 → Phase 2: 系统管理 → Phase 3: 基础数据
                                                    ↓
                          Phase 7: 收费管理 ← Phase 4: 门诊管理
                                ↓                    ↓
                          Phase 8: 统计报表    Phase 5: 住院管理
                                                    ↓
                                            Phase 6: 药房管理
                                                    ↓
                                            Phase 9: 测试优化
```

---

## 12. 附录

### 12.1 枚举定义清单

| 枚举名称 | 值定义 |
|----------|--------|
| GenderEnum | Female=0, Male=1 |
| StatusEnum | Disabled=0, Enabled=1 |
| RegistrationTypeEnum | Normal=1, Expert=2, Emergency=3 |
| RegistrationStatusEnum | Registered=1, Consulted=2, Refunded=3 |
| PrescriptionStatusEnum | Issued=1, Charged=2, Dispensed=3, Refunded=4 |
| InpatientStatusEnum | InHospital=1, Discharged=2, Transferred=3 |
| BedStatusEnum | Available=1, Occupied=2, Maintenance=3 |
| MedicalOrderTypeEnum | LongTerm=1, Temporary=2 |
| MedicalOrderStatusEnum | Issued=1, Executing=2, Completed=3, Stopped=4 |
| ChargeTypeEnum | Registration=1, Outpatient=2, Deposit=3, Settlement=4 |
| PaymentMethodEnum | Cash=1, WeChat=2, Alipay=3, BankCard=4, MedicalInsurance=5 |
| DoctorTitleEnum | Chief=1, ViceChief=2, Attending=3, Resident=4 |
| MenuTypeEnum | Directory=1, Menu=2, Button=3 |

### 12.2 编号生成规则

| 编号类型 | 格式 | 示例 |
|----------|------|------|
| 挂号单号 | GH + yyyyMMdd + 4位流水 | GH202606130001 |
| 处方号 | CF + yyyyMMdd + 4位流水 | CF202606130001 |
| 住院号 | ZY + yyyyMMdd + 4位流水 | ZY202606130001 |
| 收费单号 | SF + yyyyMMdd + 4位流水 | SF202606130001 |
| 患者编号 | HZ + 当前时间戳 | HZ1718280000 |
| 医生工号 | YS + 4位流水 | YS0001 |
| 药品编码 | YP + 8位流水 | YP00000001 |

### 12.3 GIS 验证规则参考

| 字段 | 规则 |
|------|------|
| 手机号 | 11位数字，1开头 |
| 身份证号 | 18位，符合身份证校验规则 |
| 密码 | 8-20位，含字母+数字 |
| 年龄 | 0-150 |
| 数量 | 大于0的正整数 |
| 金额 | 保留2位小数，≥0 |
| 日期 | 不超过当前日期(出生日期除外) |

---

> **文档维护说明**: 本文档为医院管理系统整体设计文档，随项目开发进程持续更新。各模块详细设计文档可作为独立文件进行补充。
