-- ============================================
-- 医院管理系统(HIS) - 存储过程
-- 数据库：SQL Server 2019+
-- ============================================

USE [HospitalDB]
GO

-- ============================================
-- 1. 日收入报表（按收费类型/支付方式汇总）
-- ============================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetDailyRevenueReport')
    DROP PROCEDURE sp_GetDailyRevenueReport
GO

CREATE PROCEDURE sp_GetDailyRevenueReport
    @ReportDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @ReportDate IS NULL SET @ReportDate = CAST(GETDATE() AS DATE);

    -- 按收费类型汇总
    SELECT ChargeType,
           CASE ChargeType WHEN 1 THEN N'挂号费'
                           WHEN 2 THEN N'门诊收费'
                           WHEN 3 THEN N'住院预交'
                           WHEN 4 THEN N'住院结算' END AS TypeName,
           COUNT(*) AS TotalCount,
           SUM(PaidAmount) AS TotalAmount
    FROM ChargeRecord
    WHERE CAST(CreateTime AS DATE) = @ReportDate AND Status = 1
    GROUP BY ChargeType

    UNION ALL

    -- 按支付方式汇总
    SELECT 9 AS ChargeType, N'---支付方式---' AS TypeName, NULL, NULL WHERE 1=0  -- 分隔标记
    UNION ALL
    SELECT PaymentMethod + 10,
           CASE PaymentMethod WHEN 1 THEN N'现金' WHEN 2 THEN N'微信'
                              WHEN 3 THEN N'支付宝' WHEN 4 THEN N'银行卡' WHEN 5 THEN N'医保' END,
           COUNT(*), SUM(PaidAmount)
    FROM ChargeRecord
    WHERE CAST(CreateTime AS DATE) = @ReportDate AND Status = 1
    GROUP BY PaymentMethod

    ORDER BY ChargeType;
END
GO

-- ============================================
-- 2. 医生工作量统计（指定日期范围）
-- ============================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetDoctorWorkload')
    DROP PROCEDURE sp_GetDoctorWorkload
GO

CREATE PROCEDURE sp_GetDoctorWorkload
    @StartDate DATE,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @EndDate IS NULL SET @EndDate = @StartDate;

    SELECT d.Id AS DoctorId, d.Name AS DoctorName, d.DoctorNo,
           dept.DeptName AS DepartmentName,
           COUNT(r.Id) AS RegistrationCount,
           COUNT(o.Id) AS ConsultationCount,
           COUNT(p.Id) AS PrescriptionCount
    FROM DoctorInfo d
    INNER JOIN SysDepartment dept ON d.DepartmentId = dept.Id
    LEFT JOIN Registration r ON r.DoctorId = d.Id
        AND r.VisitDate BETWEEN @StartDate AND @EndDate
    LEFT JOIN OutpatientRecord o ON o.DoctorId = d.Id
        AND o.VisitTime BETWEEN @StartDate AND @EndDate
    LEFT JOIN Prescription p ON p.DoctorId = d.Id
        AND p.CreateTime BETWEEN @StartDate AND @EndDate
    WHERE d.Status = 1
    GROUP BY d.Id, d.Name, d.DoctorNo, dept.DeptName
    ORDER BY ConsultationCount DESC;
END
GO

-- ============================================
-- 3. 库存预警（低于最低库存的药品）
-- ============================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetDrugStockAlert')
    DROP PROCEDURE sp_GetDrugStockAlert
GO

CREATE PROCEDURE sp_GetDrugStockAlert
AS
BEGIN
    SET NOCOUNT ON;

    SELECT d.Id, d.DrugCode, d.DrugName, d.Specification, d.Unit,
           d.StockQuantity, d.MinStock, d.RetailPrice,
           dc.CategoryName,
           CASE WHEN d.StockQuantity = 0 THEN N'缺货'
                WHEN d.StockQuantity <= d.MinStock / 2 THEN N'紧急'
                ELSE N'不足' END AS AlertLevel
    FROM DrugInfo d
    LEFT JOIN DrugCategory dc ON d.CategoryId = dc.Id
    WHERE d.Status = 1 AND d.StockQuantity <= d.MinStock
    ORDER BY d.StockQuantity ASC;
END
GO

-- ============================================
-- 4. 科室就诊统计（指定月份）
-- ============================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetDepartmentVisitStats')
    DROP PROCEDURE sp_GetDepartmentVisitStats
GO

CREATE PROCEDURE sp_GetDepartmentVisitStats
    @YearMonth CHAR(7) = NULL  -- 格式: '2026-06'
AS
BEGIN
    SET NOCOUNT ON;
    IF @YearMonth IS NULL SET @YearMonth = FORMAT(GETDATE(), 'yyyy-MM');

    DECLARE @StartDate DATE = DATEFROMPARTS(LEFT(@YearMonth,4), RIGHT(@YearMonth,2), 1);
    DECLARE @EndDate DATE = DATEADD(DAY, -1, DATEADD(MONTH, 1, @StartDate));

    SELECT dept.Id AS DeptId, dept.DeptName,
           COUNT(DISTINCT r.Id) AS VisitCount,
           COUNT(DISTINCT o.Id) AS ConsultCount,
           ISNULL(SUM(c.PaidAmount), 0) AS TotalRevenue
    FROM SysDepartment dept
    LEFT JOIN Registration r ON r.DepartmentId = dept.Id
        AND r.VisitDate BETWEEN @StartDate AND @EndDate
    LEFT JOIN OutpatientRecord o ON o.PatientId = r.PatientId
        AND o.VisitTime BETWEEN @StartDate AND @EndDate
    LEFT JOIN ChargeRecord c ON c.PatientId = r.PatientId
        AND c.CreateTime BETWEEN @StartDate AND @EndDate
        AND c.Status = 1
    WHERE dept.Status = 1 AND dept.DeptType = 1  -- 临床科室
    GROUP BY dept.Id, dept.DeptName
    ORDER BY VisitCount DESC;
END
GO

-- ============================================
-- 5. 处方发药汇总（指定日期范围，按药品统计）
-- ============================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetPrescriptionDrugSummary')
    DROP PROCEDURE sp_GetPrescriptionDrugSummary
GO

CREATE PROCEDURE sp_GetPrescriptionDrugSummary
    @StartDate DATE,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @EndDate IS NULL SET @EndDate = @StartDate;

    SELECT pd.ItemName AS DrugName, pd.Specification, pd.Unit,
           SUM(pd.Quantity) AS TotalQuantity,
           SUM(pd.Amount) AS TotalAmount,
           COUNT(DISTINCT p.Id) AS PrescriptionCount
    FROM PrescriptionDetail pd
    INNER JOIN Prescription p ON pd.PrescriptionId = p.Id
    WHERE p.CreateTime BETWEEN @StartDate AND @EndDate
        AND pd.ItemType = 1  -- 药品
        AND p.Status IN (2, 3)  -- 已收费或已发药
    GROUP BY pd.ItemName, pd.Specification, pd.Unit
    ORDER BY TotalQuantity DESC;
END
GO

PRINT N'存储过程创建完成：';
PRINT N'  1. sp_GetDailyRevenueReport      - 日收入报表';
PRINT N'  2. sp_GetDoctorWorkload          - 医生工作量统计';
PRINT N'  3. sp_GetDrugStockAlert          - 库存预警';
PRINT N'  4. sp_GetDepartmentVisitStats    - 科室就诊统计';
PRINT N'  5. sp_GetPrescriptionDrugSummary - 处方发药汇总';
GO
