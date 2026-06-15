using HIS.Models;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly IBaseRepository<Schedule> _repo;
        private readonly IBaseRepository<DoctorInfo> _docRepo;
        private readonly IUnitOfWork _uow;

        public ScheduleController(IBaseRepository<Schedule> repo,
            IBaseRepository<DoctorInfo> docRepo, IUnitOfWork uow)
        { _repo = repo; _docRepo = docRepo; _uow = uow; }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, long? departmentId = null)
        {
            var q = _repo.GetQueryable().Include(s => s.Doctor).Include(s => s.Department).AsQueryable();
            if (departmentId.HasValue) q = q.Where(s => s.DepartmentId == departmentId.Value);
            if (!string.IsNullOrEmpty(keyword))
                q = q.Where(s => s.Doctor!.Name.Contains(keyword) || s.Department!.DeptName.Contains(keyword));
            var total = await q.CountAsync();
            var list = await q.OrderBy(s => s.DayOfWeek).ThenBy(s => s.DepartmentId)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return LayuiTableResult.Ok(total, list.Select(s => new {
                s.Id, s.DoctorId, DoctorName = s.Doctor!.Name, s.DepartmentId,
                DeptName = s.Department!.DeptName, s.DayOfWeek, s.TimeSlot,
                s.MaxPatients, s.Status
            }));
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _docRepo.GetQueryable().Include(d => d.Department)
                .Select(d => d.Department).Distinct().Where(d => d != null).ToListAsync();
            ViewBag.Doctors = await _docRepo.GetQueryable().Include(d => d.Department).ToListAsync();
            return View("Create", new Schedule { Status = 1, MaxPatients = 30, TimeSlot = "全天" });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] Schedule s)
        {
            if (s.DoctorId <= 0 || s.DepartmentId <= 0) return ApiResult.Fail("请选择医生和科室");
            if (await _repo.AnyAsync(x => x.DoctorId == s.DoctorId && x.DayOfWeek == s.DayOfWeek && x.TimeSlot == s.TimeSlot && x.Status == 1))
                return ApiResult.Fail("该医生同一星期相同时段已有排班");
            s.CreateTime = DateTime.Now;
            await _repo.AddAsync(s);
            await _uow.SaveChangesAsync();
            return ApiResult.Success("排班添加成功");
        }

        public async Task<IActionResult> Edit(long id)
        {
            var s = await _repo.GetQueryable().Include(s => s.Doctor).ThenInclude(d => d!.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (s == null) return NotFound();
            ViewBag.Departments = await _docRepo.GetQueryable().Include(d => d.Department)
                .Select(d => d.Department).Distinct().Where(d => d != null).ToListAsync();
            ViewBag.Doctors = await _docRepo.GetQueryable().Include(d => d.Department).ToListAsync();
            return View("Create", s);
        }

        [HttpPost]
        public async Task<ApiResult> Edit([FromBody] Schedule s)
        { var e = await _repo.GetByIdAsync(s.Id); if (e == null) return ApiResult.Fail("不存在"); e.DoctorId = s.DoctorId; e.DepartmentId = s.DepartmentId; e.DayOfWeek = s.DayOfWeek; e.TimeSlot = s.TimeSlot; e.MaxPatients = s.MaxPatients; e.Status = s.Status; _repo.Update(e); await _uow.SaveChangesAsync(); return ApiResult.Success("更新成功"); }

        /// <summary>遗传算法智能排班</summary>
        [HttpPost]
        public async Task<ApiResult> AutoGenerate()
        {
            var doctors = await _docRepo.GetQueryable().Include(d => d.Department).Where(d => d.Status == 1).ToListAsync();
            var deptIds = doctors.Select(d => d.DepartmentId).Distinct().ToList();
            if (!doctors.Any()) return ApiResult.Fail("没有在岗医生");

            var ga = new HIS.Services.Helpers.ScheduleGA(doctors, deptIds);
            var schedules = ga.Run();

            // 清除旧排班，写入新排班
            var oldSchedules = await _repo.GetQueryable().ToListAsync();
            foreach (var s in oldSchedules) _repo.Delete(s);
            foreach (var s in schedules) await _repo.AddAsync(s);
            await _uow.SaveChangesAsync();

            return ApiResult.Success($"遗传算法完成：生成了 {schedules.Count} 条排班");
        }

        [HttpPost]
        public async Task<ApiResult> Delete([FromBody] long id)
        { var s = await _repo.GetByIdAsync(id); if (s == null) return ApiResult.Fail("不存在"); _repo.Delete(s); await _uow.SaveChangesAsync(); return ApiResult.Success("已删除"); }
    }
}
