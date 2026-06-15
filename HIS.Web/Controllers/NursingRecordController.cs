using HIS.Models;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HIS.Web.Controllers
{
    [Authorize]
    public class NursingRecordController : Controller
    {
        private readonly IBaseRepository<NursingRecord> _repo;
        private readonly IBaseRepository<InpatientRecord> _inpatientRepo;
        private readonly IUnitOfWork _uow;
        private readonly ISysLogService _log;

        public NursingRecordController(IBaseRepository<NursingRecord> repo,
            IBaseRepository<InpatientRecord> inpatientRepo, IUnitOfWork uow, ISysLogService log)
        { _repo = repo; _inpatientRepo = inpatientRepo; _uow = uow; _log = log; }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<LayuiTableResult> GetList(int pageIndex = 1, int pageSize = 10,
            string? keyword = null, long? inpatientId = null)
        {
            var q = _repo.GetQueryable()
                .Include(r => r.Patient).Include(r => r.Nurse)
                .Include(r => r.InpatientRecord).ThenInclude(i => i!.Bed)
                .AsQueryable();
            if (inpatientId.HasValue) q = q.Where(r => r.InpatientId == inpatientId.Value);
            if (!string.IsNullOrEmpty(keyword)) q = q.Where(r => r.Content.Contains(keyword));
            var total = await q.CountAsync();
            var list = await q.OrderBy(r => r.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(r => new {
                    r.Id, r.Content, r.RecordTime, r.CreateTime,
                    PatientName = r.Patient!.Name,
                    NurseName = r.Nurse!.RealName,
                    BedNo = r.InpatientRecord!.Bed!.BedNo,
                    RoomNo = r.InpatientRecord.Bed.RoomNo,
                    InpatientNo = r.InpatientRecord.InpatientNo
                }).ToListAsync();
            return LayuiTableResult.Ok(total, list);
        }

        public async Task<IActionResult> Create(long inpatientId)
        {
            var inpatient = await _inpatientRepo.FirstOrDefaultAsync(i => i.Id == inpatientId);
            return View(new NursingRecord
            {
                InpatientId = inpatientId,
                PatientId = inpatient?.PatientId ?? 0,
                RecordTime = DateTime.Now
            });
        }

        [HttpPost]
        public async Task<ApiResult> Create([FromBody] NursingRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.Content)) return ApiResult.Fail("护理内容不能为空");
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            record.NurseId = uid != null ? long.Parse(uid) : 0;
            record.CreateTime = DateTime.Now;
            await _repo.AddAsync(record);
            await _uow.SaveChangesAsync();
            await LogAsync("住院管理", "护理记录", record.Content);
            return ApiResult.Success("护理记录已保存");
        }

        private async Task LogAsync(string m, string a, string? d)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _log.LogAsync(uid != null ? long.Parse(uid) : null,
                User.Identity?.Name, m, a, d, HttpContext.Request.Path, ip);
        }
    }
}
