using HIS.Common.Helpers;
using HIS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Data
{
    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public static class DbInitializer
    {
        public static void Seed(HisDbContext context)
        {
            // 确保数据库已创建
            context.Database.EnsureCreated();

            // 关闭 IDENTITY 缓存（仅 SQL Server；InMemory 跳过）
            try { context.Database.ExecuteSqlRaw(
                "IF EXISTS (SELECT 1 FROM sys.databases WHERE name = DB_NAME() AND is_read_only = 0) " +
                "BEGIN TRY ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF; END TRY BEGIN CATCH END CATCH"); }
            catch (Exception) { /* InMemory / 非关系型数据库，跳过 */ }

            // 仅首次初始化时写入种子数据
            if (context.SysUsers.Any())
            {
                Console.WriteLine("[HIS Seed] 数据库已有数据，跳过种子初始化");
                return;
            }
            Console.WriteLine("[HIS Seed] 首次初始化，创建种子数据...");

            // 使用事务确保数据一致性（InMemory 不支持事务，跳过）
            var transaction = context.Database.IsRelational()
                ? context.Database.BeginTransaction() : null;
            try
            {
                // 1. 创建默认角色
                var adminRole = new SysRole
                {
                    RoleName = "超级管理员",
                    RoleCode = "super_admin",
                    Description = "系统超级管理员，拥有所有权限",
                    SortOrder = 1,
                    Status = 1,
                    CreateTime = DateTime.Now
                };
                context.SysRoles.Add(adminRole);
                context.SaveChanges();

                var doctorRole = new SysRole
                {
                    RoleName = "医生",
                    RoleCode = "doctor",
                    Description = "门诊/住院医生",
                    SortOrder = 2,
                    Status = 1,
                    CreateTime = DateTime.Now
                };
                context.SysRoles.Add(doctorRole);
                context.SaveChanges();

                // 2. 创建菜单（首页作为独立菜单，可被权限管理）
                CreateMenu(context, null, "首页", 2, "layui-icon-home", "", "/Home/Index", 0);

                // 系统管理
                var systemMenu = CreateMenu(context, null, "系统管理", 1, "layui-icon-set", "", "", 1);
                var userMenu = CreateMenu(context, systemMenu.Id, "用户管理", 2, "layui-icon-user", "sys:user", "/SysUser/Index", 1);
                CreateMenu(context, userMenu.Id, "新增用户", 3, "", "sys:user:add", "", 1);
                CreateMenu(context, userMenu.Id, "编辑用户", 3, "", "sys:user:edit", "", 2);
                CreateMenu(context, userMenu.Id, "删除用户", 3, "", "sys:user:delete", "", 3);

                var roleMenu = CreateMenu(context, systemMenu.Id, "角色管理", 2, "layui-icon-auz", "sys:role", "/SysRole/Index", 2);
                CreateMenu(context, roleMenu.Id, "新增角色", 3, "", "sys:role:add", "", 1);
                CreateMenu(context, roleMenu.Id, "权限分配", 3, "", "sys:role:permission", "", 2);

                var menuMenu = CreateMenu(context, systemMenu.Id, "菜单管理", 2, "layui-icon-tree", "sys:menu", "/SysMenu/Index", 3);

                var deptMenu = CreateMenu(context, systemMenu.Id, "科室管理", 2, "layui-icon-component", "sys:dept", "/SysDepartment/Index", 4);

                var logMenu = CreateMenu(context, systemMenu.Id, "操作日志", 2, "layui-icon-file-b", "sys:log", "/SysLog/Index", 5);
                CreateMenu(context, systemMenu.Id, "系统配置", 2, "layui-icon-set", "sys:config", "/SysConfig/Index", 6);

                // 基础数据菜单
                var baseDataMenu = CreateMenu(context, null, "基础数据", 1, "layui-icon-template-1", "", "", 2);
                CreateMenu(context, baseDataMenu.Id, "患者管理", 2, "layui-icon-username", "patient", "/Patient/Index", 1);
                CreateMenu(context, baseDataMenu.Id, "医生管理", 2, "layui-icon-user", "doctor", "/Doctor/Index", 2);
                CreateMenu(context, baseDataMenu.Id, "药品字典", 2, "layui-icon-form", "drug", "/Drug/Index", 3);
                CreateMenu(context, baseDataMenu.Id, "药品分类", 2, "layui-icon-tree", "drug:category", "/DrugCategory/Index", 4);
                CreateMenu(context, baseDataMenu.Id, "检查项目", 2, "layui-icon-form", "charge:item", "/ChargeItem/Index", 5);

                // 门诊管理菜单
                var outpatientMenu = CreateMenu(context, null, "门诊管理", 1, "layui-icon-read", "", "", 3);
                CreateMenu(context, outpatientMenu.Id, "挂号管理", 2, "layui-icon-flag", "registration", "/Registration/Index", 1);
                CreateMenu(context, outpatientMenu.Id, "门诊诊疗", 2, "layui-icon-survey", "outpatient", "/OutpatientRecord/Index", 2);
                CreateMenu(context, outpatientMenu.Id, "处方管理", 2, "layui-icon-edit", "prescription", "/Prescription/Index", 3);
                CreateMenu(context, outpatientMenu.Id, "排班管理", 2, "layui-icon-date", "schedule", "/Schedule/Index", 4);

                // 住院管理菜单
                var inpatientMenu = CreateMenu(context, null, "住院管理", 1, "layui-icon-home", "", "", 4);
                CreateMenu(context, inpatientMenu.Id, "入院登记", 2, "layui-icon-add-circle", "inpatient", "/InpatientRecord/Index", 1);
                CreateMenu(context, inpatientMenu.Id, "床位管理", 2, "layui-icon-cellphone", "bed", "/BedInfo/Index", 2);
                CreateMenu(context, inpatientMenu.Id, "医嘱管理", 2, "layui-icon-list", "order", "/MedicalOrder/Index", 3);
                CreateMenu(context, inpatientMenu.Id, "护理记录", 2, "layui-icon-edit", "nursing", "/NursingRecord/Index", 4);

                // 药房管理菜单
                var pharmacyMenu = CreateMenu(context, null, "药房管理", 1, "layui-icon-component", "", "", 5);
                CreateMenu(context, pharmacyMenu.Id, "库存管理", 2, "layui-icon-chart", "stock", "/DrugStock/Index", 1);
                CreateMenu(context, pharmacyMenu.Id, "处方发药", 2, "layui-icon-form", "drug:dispense", "/Dispense/Index", 2);

                // 收费管理菜单
                var financeMenu = CreateMenu(context, null, "收费管理", 1, "layui-icon-rmb", "", "", 6);
                CreateMenu(context, financeMenu.Id, "收费流水", 2, "layui-icon-dollar", "charge", "/Charge/Index", 1);

                // 统计报表菜单
                var statsMenu = CreateMenu(context, null, "统计报表", 1, "layui-icon-chart-screen", "", "", 7);
                CreateMenu(context, statsMenu.Id, "统计报表", 2, "layui-icon-chart-screen", "stats", "/Statistics/Index", 1);

                context.SaveChanges();

                // 3. 为超级管理员角色分配所有菜单
                var allMenus = context.SysMenus.ToList();
                Console.WriteLine($"[HIS Seed] 共创建 {allMenus.Count} 个菜单项");
                Console.WriteLine($"[HIS Seed] 超级管理员(ID={adminRole.Id})分配了全部 {allMenus.Count} 个菜单");
                foreach (var menu in allMenus)
                {
                    context.SysRoleMenus.Add(new SysRoleMenu { RoleId = adminRole.Id, MenuId = menu.Id });
                }
                // 为医生角色分配菜单：首页 + 门诊(诊疗+处方) + 住院(医嘱)
                var doctorMenuIds = context.SysMenus
                    .Where(m =>
                        // 首页（所有角色都有）
                        m.MenuName == "首页" ||
                        // 医生目录：门诊管理、住院管理
                        (m.MenuType == 1 && m.MenuUrl == "" && (
                            m.MenuName.Contains("门诊") || m.MenuName.Contains("住院"))) ||
                        // 医生页面：门诊诊疗、处方管理、医嘱管理
                        (m.MenuUrl != null && m.MenuUrl != "" && (
                            m.MenuUrl.Contains("OutpatientRecord") ||
                            m.MenuUrl.Contains("Prescription") ||
                            m.MenuUrl.Contains("MedicalOrder"))))
                    .Select(m => m.Id)
                    .ToList();
                foreach (var menuId in doctorMenuIds)
                {
                    context.SysRoleMenus.Add(new SysRoleMenu { RoleId = doctorRole.Id, MenuId = menuId });
                }
                context.SaveChanges();

                // 输出医生菜单分配结果到控制台，方便调试
                Console.WriteLine($"[HIS Seed] 医生角色(ID={doctorRole.Id})分配了 {doctorMenuIds.Count} 个菜单项:");
                foreach (var mid in doctorMenuIds)
                {
                    var m = context.SysMenus.Find(mid);
                    Console.WriteLine($"  - [{m?.MenuType}] {m?.MenuName} (URL={m?.MenuUrl})");
                }

                // 4. 创建默认科室
                var depts = new[]
                {
                    new SysDepartment { DeptName = "内科", DeptCode = "NK", DeptType = 1, SortOrder = 1, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "外科", DeptCode = "WK", DeptType = 1, SortOrder = 2, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "儿科", DeptCode = "EK", DeptType = 1, SortOrder = 3, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "妇产科", DeptCode = "FCK", DeptType = 1, SortOrder = 4, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "骨科", DeptCode = "GK", DeptType = 1, SortOrder = 5, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "眼科", DeptCode = "YK", DeptType = 1, SortOrder = 6, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "耳鼻喉科", DeptCode = "EBH", DeptType = 1, SortOrder = 7, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "皮肤科", DeptCode = "PFK", DeptType = 1, SortOrder = 8, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "检验科", DeptCode = "JYK", DeptType = 2, SortOrder = 9, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "放射科", DeptCode = "FSK", DeptType = 2, SortOrder = 10, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "药房", DeptCode = "YF", DeptType = 2, SortOrder = 11, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "收费处", DeptCode = "SFC", DeptType = 3, SortOrder = 12, Status = 1, CreateTime = DateTime.Now },
                    new SysDepartment { DeptName = "急诊科", DeptCode = "JZK", DeptType = 1, SortOrder = 13, Status = 1, CreateTime = DateTime.Now },
                };
                context.SysDepartments.AddRange(depts);
                context.SaveChanges();

                // 5. 创建默认管理员用户
                var salt = EncryptionHelper.GenerateSalt();
                var adminUser = new SysUser
                {
                    UserName = "admin",
                    Password = EncryptionHelper.HashPassword("123456", salt),
                    Salt = salt,
                    RealName = "系统管理员",
                    Gender = 1,
                    Phone = "13800000000",
                    Email = "admin@hospital.com",
                    RoleId = adminRole.Id,
                    Status = 1,
                    CreateTime = DateTime.Now
                };
                context.SysUsers.Add(adminUser);
                context.SaveChanges();

                // 6. 创建测试医生用户
                var doctorSalt = EncryptionHelper.GenerateSalt();
                var doctorUser = new SysUser
                {
                    UserName = "doctor01",
                    Password = EncryptionHelper.HashPassword("123456", doctorSalt),
                    Salt = doctorSalt,
                    RealName = "张医生",
                    Gender = 1,
                    Phone = "13800000001",
                    RoleId = doctorRole.Id,
                    DepartmentId = depts[0].Id,
                    Status = 1,
                    CreateTime = DateTime.Now
                };
                context.SysUsers.Add(doctorUser);
                context.SaveChanges();

                // 7. 药品分类种子数据
                var drugCategories = new[]
                {
                    new DrugCategory { CategoryName = "西药", SortOrder = 1 },
                    new DrugCategory { CategoryName = "中成药", SortOrder = 2 },
                    new DrugCategory { CategoryName = "中药饮片", SortOrder = 3 }
                };
                context.DrugCategories.AddRange(drugCategories);
                context.SaveChanges();
                // 二级分类
                var westernDrug = drugCategories[0];
                var antiBio = new DrugCategory { CategoryName = "抗生素", ParentId = westernDrug.Id, SortOrder = 1 };
                var cardio = new DrugCategory { CategoryName = "心血管用药", ParentId = westernDrug.Id, SortOrder = 2 };
                var digestive = new DrugCategory { CategoryName = "消化系统用药", ParentId = westernDrug.Id, SortOrder = 3 };
                var respiratory = new DrugCategory { CategoryName = "呼吸系统用药", ParentId = westernDrug.Id, SortOrder = 4 };
                var analgesic = new DrugCategory { CategoryName = "解热镇痛药", ParentId = westernDrug.Id, SortOrder = 5 };
                context.DrugCategories.AddRange(antiBio, cardio, digestive, respiratory, analgesic);
                context.SaveChanges();
                // 三级分类
                var cephalosporin = new DrugCategory { CategoryName = "头孢菌素类", ParentId = antiBio.Id, SortOrder = 1 };
                var penicillin = new DrugCategory { CategoryName = "青霉素类", ParentId = antiBio.Id, SortOrder = 2 };
                var macrolide = new DrugCategory { CategoryName = "大环内酯类", ParentId = antiBio.Id, SortOrder = 3 };
                context.DrugCategories.AddRange(cephalosporin, penicillin, macrolide);
                context.SaveChanges();

                // 8. 药品种子数据
                var drugs = new[]
                {
                    new DrugInfo { DrugCode = "YP00000001", DrugName = "阿莫西林胶囊", GenericName = "阿莫西林", CategoryId = penicillin.Id, Specification = "0.5g*24粒", Unit = "盒", Manufacturer = "华北制药", UnitPrice = 8.50m, RetailPrice = 12.00m, StockQuantity = 200, MinStock = 20, IsPrescription = true, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000002", DrugName = "头孢克肟分散片", GenericName = "头孢克肟", CategoryId = cephalosporin.Id, Specification = "0.1g*6片", Unit = "盒", Manufacturer = "广州白云山", UnitPrice = 15.00m, RetailPrice = 22.50m, StockQuantity = 150, MinStock = 15, IsPrescription = true, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000003", DrugName = "阿奇霉素片", GenericName = "阿奇霉素", CategoryId = macrolide.Id, Specification = "0.25g*6片", Unit = "盒", Manufacturer = "辉瑞制药", UnitPrice = 28.00m, RetailPrice = 42.00m, StockQuantity = 100, MinStock = 10, IsPrescription = true, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000004", DrugName = "硝苯地平缓释片", GenericName = "硝苯地平", CategoryId = cardio.Id, Specification = "30mg*7片", Unit = "盒", Manufacturer = "拜耳医药", UnitPrice = 25.00m, RetailPrice = 35.00m, StockQuantity = 180, MinStock = 20, IsPrescription = true, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000005", DrugName = "奥美拉唑肠溶胶囊", GenericName = "奥美拉唑", CategoryId = digestive.Id, Specification = "20mg*14粒", Unit = "盒", Manufacturer = "阿斯利康", UnitPrice = 32.00m, RetailPrice = 48.00m, StockQuantity = 120, MinStock = 15, IsPrescription = true, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000006", DrugName = "布洛芬缓释胶囊", GenericName = "布洛芬", CategoryId = analgesic.Id, Specification = "0.3g*20粒", Unit = "盒", Manufacturer = "中美史克", UnitPrice = 10.00m, RetailPrice = 18.00m, StockQuantity = 300, MinStock = 30, IsPrescription = false, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000007", DrugName = "氨溴索口服液", GenericName = "盐酸氨溴索", CategoryId = respiratory.Id, Specification = "100ml:0.6g", Unit = "瓶", Manufacturer = "勃林格殷格翰", UnitPrice = 18.00m, RetailPrice = 28.00m, StockQuantity = 90, MinStock = 10, IsPrescription = false, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000008", DrugName = "复方丹参滴丸", GenericName = "复方丹参", CategoryId = drugCategories[1].Id, Specification = "27mg*180丸", Unit = "瓶", Manufacturer = "天士力", UnitPrice = 20.00m, RetailPrice = 29.80m, StockQuantity = 160, MinStock = 20, IsPrescription = false, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000009", DrugName = "板蓝根颗粒", GenericName = "板蓝根", CategoryId = drugCategories[1].Id, Specification = "10g*20袋", Unit = "包", Manufacturer = "白云山", UnitPrice = 8.00m, RetailPrice = 15.00m, StockQuantity = 400, MinStock = 50, IsPrescription = false, Status = 1, CreateTime = DateTime.Now },
                    new DrugInfo { DrugCode = "YP00000010", DrugName = "连花清瘟胶囊", GenericName = "连花清瘟", CategoryId = drugCategories[1].Id, Specification = "0.35g*36粒", Unit = "盒", Manufacturer = "以岭药业", UnitPrice = 12.00m, RetailPrice = 22.00m, StockQuantity = 250, MinStock = 30, IsPrescription = false, Status = 1, CreateTime = DateTime.Now },
                };
                context.DrugInfos.AddRange(drugs);
                context.SaveChanges();

                // 9. 检查项目种子数据
                var chargeItems = new[]
                {
                    new ChargeItem { Category = "CT", ItemName = "头颅CT平扫", UnitPrice = 250, Unit = "次", Description = "头部", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "胸部CT平扫", UnitPrice = 300, Unit = "次", Description = "肺部、纵隔", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "腹部CT平扫", UnitPrice = 350, Unit = "次", Description = "肝、胆、胰、脾、肾", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "盆腔CT平扫", UnitPrice = 350, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "颈椎CT平扫", UnitPrice = 280, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "腰椎CT平扫", UnitPrice = 280, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "CT", ItemName = "CT增强扫描（任一部位）", Specification = "需注射造影剂", UnitPrice = 500, Unit = "次", Description = "在平扫基础上加收200-350", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "头颅MRI平扫", UnitPrice = 550, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "颈椎MRI平扫", UnitPrice = 550, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "腰椎MRI平扫", UnitPrice = 550, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "膝关节MRI平扫", UnitPrice = 500, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "腹部MRI平扫", UnitPrice = 650, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "MRI", ItemName = "MRI增强扫描（任一部位）", Specification = "需造影剂", UnitPrice = 850, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "胸部正位片", UnitPrice = 90, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "胸部正侧位片", UnitPrice = 140, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "腹部立位平片", UnitPrice = 100, Unit = "次", Description = "肠梗阻、穿孔急查", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "颈椎正侧位片", UnitPrice = 100, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "腰椎正侧位片", UnitPrice = 100, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "X线(DR)", ItemName = "四肢骨关节X线（单侧）", UnitPrice = 90, Unit = "次", Description = "含腕、肘、肩、膝、踝等", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "腹部彩超（肝胆胰脾）", UnitPrice = 160, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "泌尿系彩超（双肾+输尿管+膀胱）", UnitPrice = 160, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "妇科彩超（子宫附件）", UnitPrice = 160, Unit = "次", Description = "经腹或经阴道", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "产科彩超（胎儿）", UnitPrice = 220, Unit = "次", Description = "普通产检", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "甲状腺彩超", UnitPrice = 140, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "心脏彩超（超声心动图）", UnitPrice = 280, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "超声", ItemName = "乳腺彩超", UnitPrice = 140, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "PET-CT", ItemName = "PET-CT（全身）", UnitPrice = 7500, Unit = "次", Description = "肿瘤检查，价格较高", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "核医学", ItemName = "骨显像（全身骨扫描）", UnitPrice = 650, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "内镜", ItemName = "普通胃镜", Specification = "不含麻醉", UnitPrice = 300, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "内镜", ItemName = "无痛胃镜", Specification = "含麻醉费", UnitPrice = 800, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "内镜", ItemName = "普通肠镜", Specification = "不含麻醉", UnitPrice = 400, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "内镜", ItemName = "无痛肠镜", Specification = "含麻醉费", UnitPrice = 950, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "心电", ItemName = "常规心电图", UnitPrice = 30, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                    new ChargeItem { Category = "心电", ItemName = "动态心电图（24小时）", UnitPrice = 280, Unit = "次", Status = 1, CreateTime = DateTime.Now },
                };
                context.ChargeItems.AddRange(chargeItems);
                context.SaveChanges();

                // 10. 床位种子数据（内科15床、外科12床）
                var beds = new List<BedInfo>();
                for (int i = 1; i <= 15; i++)
                    beds.Add(new BedInfo { BedNo = $"{i}床", RoomNo = $"30{i / 4 + 1}", DepartmentId = depts[0].Id, BedType = i <= 10 ? 1 : 2, DailyRate = i <= 10 ? 50 : 80, Status = 1, CreateTime = DateTime.Now });
                for (int i = 1; i <= 12; i++)
                    beds.Add(new BedInfo { BedNo = $"{i}床", RoomNo = $"40{i / 3 + 1}", DepartmentId = depts[1].Id, BedType = i <= 8 ? 1 : (i <= 10 ? 2 : 3), DailyRate = i <= 8 ? 50 : (i <= 10 ? 80 : 150), Status = 1, CreateTime = DateTime.Now });
                // 产科5床、儿科4床
                for (int i = 1; i <= 5; i++)
                    beds.Add(new BedInfo { BedNo = $"{i}床", RoomNo = $"50{i / 3 + 1}", DepartmentId = depts[2].Id, BedType = i <= 3 ? 2 : 3, DailyRate = i <= 3 ? 80 : 150, Status = 1, CreateTime = DateTime.Now });
                for (int i = 1; i <= 4; i++)
                    beds.Add(new BedInfo { BedNo = $"{i}床", RoomNo = $"60{i / 2 + 1}", DepartmentId = depts[3].Id, BedType = 1, DailyRate = 40, Status = 1, CreateTime = DateTime.Now });
                context.BedInfos.AddRange(beds);
                context.SaveChanges();

                // 11. 患者种子数据（10条）
                var patients = new[]
                {
                    new PatientInfo { PatientNo = "P20260601001", Name = "李明", Gender = 1, Birthday = new DateTime(1985, 3, 15), Age = 41, IdCard = "350102198503151234", Phone = "13950123456", Address = "福州市鼓楼区", BloodType = "A", AllergyHistory = "无", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601002", Name = "王芳", Gender = 0, Birthday = new DateTime(1990, 7, 22), Age = 35, IdCard = "350103199007221111", Phone = "13860123457", Address = "福州市台江区", BloodType = "B", AllergyHistory = "青霉素过敏", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601003", Name = "张伟", Gender = 1, Birthday = new DateTime(1978, 11, 8), Age = 47, IdCard = "350104197811082222", Phone = "13750123458", Address = "福州市仓山区", BloodType = "O", AllergyHistory = "无", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601004", Name = "刘娜", Gender = 0, Birthday = new DateTime(1995, 5, 30), Age = 31, IdCard = "350105199505303333", Phone = "13650123459", Address = "福州市晋安区", BloodType = "AB", AllergyHistory = "海鲜过敏", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601005", Name = "陈强", Gender = 1, Birthday = new DateTime(1970, 1, 12), Age = 56, IdCard = "350102197001124444", Phone = "13550123460", Address = "福州市马尾区", BloodType = "A", AllergyHistory = "磺胺类药物过敏", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601006", Name = "赵丽", Gender = 0, Birthday = new DateTime(1988, 9, 5), Age = 37, IdCard = "350103198809055555", Phone = "13450123461", Address = "福州市长乐区", BloodType = "B", AllergyHistory = "无", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601007", Name = "孙浩", Gender = 1, Birthday = new DateTime(2000, 12, 20), Age = 25, IdCard = "350104200012206666", Phone = "13350123462", Address = "福州市闽侯县", BloodType = "O", AllergyHistory = "花粉过敏", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601008", Name = "周静", Gender = 0, Birthday = new DateTime(1965, 8, 18), Age = 60, IdCard = "350105196508187777", Phone = "13250123463", Address = "福州市连江县", BloodType = "A", AllergyHistory = "无", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601009", Name = "吴军", Gender = 1, Birthday = new DateTime(1982, 4, 3), Age = 44, IdCard = "350102198204038888", Phone = "13150123464", Address = "福州市罗源县", BloodType = "B", AllergyHistory = "头孢过敏", CreateTime = DateTime.Now },
                    new PatientInfo { PatientNo = "P20260601010", Name = "郑敏", Gender = 0, Birthday = new DateTime(1992, 6, 28), Age = 34, IdCard = "350103199206289999", Phone = "13050123465", Address = "福州市永泰县", BloodType = "O", AllergyHistory = "无", CreateTime = DateTime.Now },
                };
                context.PatientInfos.AddRange(patients);
                context.SaveChanges();

                // 12. 各科室医生种子（每个临床科室一个）
                // DeptIdx: 0内科 1外科 2儿科 3妇产科 4骨科 5眼科 6耳鼻喉 7皮肤
                // DeptIdx: 8检验科 9放射科 10药房 11急诊
                var doctorSeeds = new[]
                {
                    // 内科 (DeptIdx=0) — 8人
                    new { UserName = "YS0002", RealName = "刘伟", Gender = 1, DeptIdx = 0, Title = 1, Specialty = "心血管内科", MaxDaily = 30, Fee = 25m },
                    new { UserName = "YS0003", RealName = "陈芳", Gender = 0, DeptIdx = 0, Title = 2, Specialty = "消化内科", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0004", RealName = "孙建", Gender = 1, DeptIdx = 0, Title = 3, Specialty = "呼吸内科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0005", RealName = "周丽", Gender = 0, DeptIdx = 0, Title = 3, Specialty = "神经内科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0006", RealName = "吴强", Gender = 1, DeptIdx = 0, Title = 4, Specialty = "内分泌科", MaxDaily = 50, Fee = 10m },
                    new { UserName = "YS0007", RealName = "郑敏", Gender = 0, DeptIdx = 0, Title = 3, Specialty = "肾内科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0008", RealName = "冯涛", Gender = 1, DeptIdx = 0, Title = 4, Specialty = "血液内科", MaxDaily = 45, Fee = 10m },
                    new { UserName = "YS0009", RealName = "蒋平", Gender = 1, DeptIdx = 0, Title = 2, Specialty = "风湿免疫科", MaxDaily = 35, Fee = 20m },
                    // 外科 (DeptIdx=1) — 7人
                    new { UserName = "YS0010", RealName = "李医生", Gender = 1, DeptIdx = 1, Title = 2, Specialty = "普外科、肝胆外科", MaxDaily = 40, Fee = 20m },
                    new { UserName = "YS0011", RealName = "赵明", Gender = 1, DeptIdx = 1, Title = 1, Specialty = "普外科、胃肠外科", MaxDaily = 30, Fee = 25m },
                    new { UserName = "YS0012", RealName = "钱进", Gender = 1, DeptIdx = 1, Title = 2, Specialty = "泌尿外科", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0013", RealName = "韩雪", Gender = 0, DeptIdx = 1, Title = 3, Specialty = "乳腺外科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0014", RealName = "杨林", Gender = 1, DeptIdx = 1, Title = 3, Specialty = "胸外科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0015", RealName = "朱勇", Gender = 1, DeptIdx = 1, Title = 4, Specialty = "血管外科", MaxDaily = 45, Fee = 10m },
                    new { UserName = "YS0016", RealName = "秦风", Gender = 0, DeptIdx = 1, Title = 4, Specialty = "整形外科", MaxDaily = 50, Fee = 10m },
                    // 儿科 (DeptIdx=2) — 5人
                    new { UserName = "YS0017", RealName = "李静", Gender = 0, DeptIdx = 2, Title = 3, Specialty = "小儿内科、新生儿", MaxDaily = 35, Fee = 15m },
                    new { UserName = "YS0018", RealName = "黄婷", Gender = 0, DeptIdx = 2, Title = 2, Specialty = "小儿呼吸科", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0019", RealName = "马超", Gender = 1, DeptIdx = 2, Title = 3, Specialty = "小儿消化科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0020", RealName = "宋雨", Gender = 0, DeptIdx = 2, Title = 4, Specialty = "新生儿科", MaxDaily = 45, Fee = 10m },
                    new { UserName = "YS0021", RealName = "段鹏", Gender = 1, DeptIdx = 2, Title = 1, Specialty = "小儿外科", MaxDaily = 30, Fee = 25m },
                    // 妇产科 (DeptIdx=3) — 5人
                    new { UserName = "YS0022", RealName = "王芳华", Gender = 0, DeptIdx = 3, Title = 2, Specialty = "产科、妇科肿瘤", MaxDaily = 30, Fee = 20m },
                    new { UserName = "YS0023", RealName = "林燕", Gender = 0, DeptIdx = 3, Title = 1, Specialty = "产科", MaxDaily = 25, Fee = 25m },
                    new { UserName = "YS0024", RealName = "何梅", Gender = 0, DeptIdx = 3, Title = 3, Specialty = "妇科", MaxDaily = 35, Fee = 15m },
                    new { UserName = "YS0025", RealName = "罗琳", Gender = 0, DeptIdx = 3, Title = 3, Specialty = "生殖内分泌", MaxDaily = 35, Fee = 15m },
                    new { UserName = "YS0026", RealName = "梁红", Gender = 0, DeptIdx = 3, Title = 4, Specialty = "产前诊断", MaxDaily = 40, Fee = 10m },
                    // 骨科 (DeptIdx=4) — 5人
                    new { UserName = "YS0027", RealName = "赵刚", Gender = 1, DeptIdx = 4, Title = 1, Specialty = "关节外科、脊柱外科", MaxDaily = 30, Fee = 25m },
                    new { UserName = "YS0028", RealName = "唐磊", Gender = 1, DeptIdx = 4, Title = 2, Specialty = "创伤骨科", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0029", RealName = "曹阳", Gender = 1, DeptIdx = 4, Title = 3, Specialty = "运动医学", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0030", RealName = "邓辉", Gender = 1, DeptIdx = 4, Title = 3, Specialty = "手外科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0031", RealName = "彭成", Gender = 1, DeptIdx = 4, Title = 4, Specialty = "骨肿瘤科", MaxDaily = 45, Fee = 10m },
                    // 眼科 (DeptIdx=5) — 4人
                    new { UserName = "YS0032", RealName = "陈明", Gender = 1, DeptIdx = 5, Title = 3, Specialty = "白内障、青光眼", MaxDaily = 50, Fee = 15m },
                    new { UserName = "YS0033", RealName = "胡子豪", Gender = 1, DeptIdx = 5, Title = 2, Specialty = "眼底病", MaxDaily = 40, Fee = 20m },
                    new { UserName = "YS0034", RealName = "余芳", Gender = 0, DeptIdx = 5, Title = 3, Specialty = "角膜病", MaxDaily = 45, Fee = 15m },
                    new { UserName = "YS0035", RealName = "潘晓", Gender = 0, DeptIdx = 5, Title = 4, Specialty = "小儿眼科", MaxDaily = 50, Fee = 10m },
                    // 耳鼻喉科 (DeptIdx=6) — 4人
                    new { UserName = "YS0036", RealName = "周文博", Gender = 1, DeptIdx = 6, Title = 4, Specialty = "鼻炎、中耳炎", MaxDaily = 45, Fee = 10m },
                    new { UserName = "YS0037", RealName = "蔡青", Gender = 0, DeptIdx = 6, Title = 3, Specialty = "咽喉头颈外科", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0038", RealName = "丁浩", Gender = 1, DeptIdx = 6, Title = 2, Specialty = "耳科", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0039", RealName = "魏兰", Gender = 0, DeptIdx = 6, Title = 4, Specialty = "听力康复", MaxDaily = 50, Fee = 10m },
                    // 皮肤科 (DeptIdx=7) — 4人
                    new { UserName = "YS0040", RealName = "林美玲", Gender = 0, DeptIdx = 7, Title = 3, Specialty = "皮炎、银屑病", MaxDaily = 40, Fee = 15m },
                    new { UserName = "YS0041", RealName = "任强", Gender = 1, DeptIdx = 7, Title = 2, Specialty = "皮肤肿瘤", MaxDaily = 35, Fee = 20m },
                    new { UserName = "YS0042", RealName = "姜瑶", Gender = 0, DeptIdx = 7, Title = 3, Specialty = "医学美容", MaxDaily = 35, Fee = 15m },
                    new { UserName = "YS0043", RealName = "廖峰", Gender = 1, DeptIdx = 7, Title = 4, Specialty = "性病科", MaxDaily = 50, Fee = 10m },
                    // 检验科 (DeptIdx=8) — 3人
                    new { UserName = "YS0044", RealName = "石磊", Gender = 1, DeptIdx = 8, Title = 2, Specialty = "临床检验", MaxDaily = 60, Fee = 15m },
                    new { UserName = "YS0045", RealName = "龚兰", Gender = 0, DeptIdx = 8, Title = 3, Specialty = "生化检验", MaxDaily = 60, Fee = 15m },
                    new { UserName = "YS0046", RealName = "尹涛", Gender = 1, DeptIdx = 8, Title = 4, Specialty = "微生物检验", MaxDaily = 60, Fee = 10m },
                    // 放射科 (DeptIdx=9) — 3人
                    new { UserName = "YS0047", RealName = "姚明辉", Gender = 1, DeptIdx = 9, Title = 2, Specialty = "影像诊断", MaxDaily = 50, Fee = 20m },
                    new { UserName = "YS0048", RealName = "龙晓", Gender = 0, DeptIdx = 9, Title = 3, Specialty = "介入放射", MaxDaily = 45, Fee = 15m },
                    new { UserName = "YS0049", RealName = "万杰", Gender = 1, DeptIdx = 9, Title = 4, Specialty = "核磁共振", MaxDaily = 50, Fee = 10m },
                    // 急诊 (DeptIdx=11) — 1人
                    new { UserName = "YS0050", RealName = "雷刚", Gender = 1, DeptIdx = 12, Title = 2, Specialty = "急诊医学", MaxDaily = 60, Fee = 20m },
                };
                foreach (var ds in doctorSeeds)
                {
                    var dsalt = EncryptionHelper.GenerateSalt();
                    var user = new SysUser { UserName = ds.UserName, Password = EncryptionHelper.HashPassword("123456", dsalt), Salt = dsalt, RealName = ds.RealName, Gender = ds.Gender, RoleId = doctorRole.Id, DepartmentId = depts[ds.DeptIdx].Id, Status = 1, CreateTime = DateTime.Now };
                    context.SysUsers.Add(user);
                    context.SaveChanges();
                    context.DoctorInfos.Add(new DoctorInfo { UserId = user.Id, DoctorNo = ds.UserName, Name = ds.RealName, Gender = ds.Gender, DepartmentId = depts[ds.DeptIdx].Id, Title = ds.Title, Specialty = ds.Specialty, MaxDailyPatients = ds.MaxDaily, ConsultationFee = ds.Fee, Status = 1, CreateTime = DateTime.Now });
                    context.SaveChanges();
                }

                transaction?.Commit();
            }
            catch
            {
                transaction?.Rollback();
                throw;
            }
        }

        private static SysMenu CreateMenu(HisDbContext context, long? parentId, string name,
            int menuType, string icon, string permissionCode, string url, int sortOrder)
        {
            var menu = new SysMenu
            {
                ParentId = parentId,
                MenuName = name,
                MenuType = menuType,
                MenuIcon = icon,
                PermissionCode = permissionCode,
                MenuUrl = url,
                SortOrder = sortOrder,
                Status = 1,
                CreateTime = DateTime.Now
            };
            context.SysMenus.Add(menu);
            context.SaveChanges();
            return menu;
        }

    }
}
