using HIS.Common.Enums;
using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 住院服务实现
    /// 核心业务：入院登记、分配床位、出院结算
    /// </summary>
    public class InpatientService : IInpatientService
    {
        private readonly IInpatientRecordRepository _inpatientRepo;
        private readonly IBedInfoRepository _bedRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IUnitOfWork _unitOfWork;

        public InpatientService(
            IInpatientRecordRepository inpatientRepo, IBedInfoRepository bedRepo,
            IPatientRepository patientRepo, IDoctorRepository doctorRepo,
            IUnitOfWork unitOfWork)
        {
            _inpatientRepo = inpatientRepo;
            _bedRepo = bedRepo;
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<InpatientRecordDto> List, int Total)> GetListAsync(InpatientQueryModel query)
        {
            var (list, total) = await _inpatientRepo.GetListAsync(query);
            return (list.Select(MapToDto).ToList(), total);
        }

        public async Task<InpatientRecordDto?> GetByIdAsync(long id)
        {
            var record = await _inpatientRepo.GetDetailAsync(id);
            return record == null ? null : MapToDto(record);
        }

        public async Task<(bool Success, string Message)> AdmitAsync(InpatientRecordDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            // 校验患者存在且未在院
            var patient = await _patientRepo.GetByIdAsync(dto.PatientId);
            if (patient == null) return (false, "患者不存在");

            var hasActive = await _inpatientRepo.HasActiveRecordAsync(dto.PatientId);
            if (hasActive) return (false, "该患者已有在院记录，不能重复入院");

            // 通过系统用户ID查找医生
            var doctor = await _doctorRepo.GetByUserIdAsync(userId);
            if (doctor == null) return (false, "当前用户不是医生");

            // 分配床位
            if (dto.BedId.HasValue)
            {
                var bed = await _bedRepo.GetByIdAsync(dto.BedId.Value);
                if (bed == null) return (false, "床位不存在");
                if (bed.Status != (int)BedStatusEnum.Available) return (false, "该床位已被占用或维修中");

                bed.Status = (int)BedStatusEnum.Occupied;
                _bedRepo.Update(bed);
            }

            var inpatientNo = GenerateNoHelper.GenerateInpatientNo();

            var record = new InpatientRecord
            {
                InpatientNo = inpatientNo,
                PatientId = dto.PatientId,
                BedId = dto.BedId,
                DepartmentId = dto.DepartmentId,
                DoctorId = doctor.Id,
                AdmissionTime = dto.AdmissionTime,
                AdmissionDiagnosis = dto.AdmissionDiagnosis,
                DepositAmount = dto.DepositAmount,
                Status = (int)InpatientStatusEnum.InHospital,
                CreateTime = DateTime.Now
            };

            await _inpatientRepo.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"入院登记成功！住院号：{inpatientNo}");
        }

        public async Task<(bool Success, string Message)> DischargeAsync(long id, string dischargeDiagnosis)
        {
            var record = await _inpatientRepo.GetByIdAsync(id);
            if (record == null) return (false, "住院记录不存在");
            if (record.Status != (int)InpatientStatusEnum.InHospital)
                return (false, "该患者已出院，不能重复操作");

            // 释放床位
            if (record.BedId.HasValue)
            {
                var bed = await _bedRepo.GetByIdAsync(record.BedId.Value);
                if (bed != null)
                {
                    bed.Status = (int)BedStatusEnum.Available;
                    _bedRepo.Update(bed);
                }
            }

            record.Status = (int)InpatientStatusEnum.Discharged;
            record.DischargeTime = DateTime.Now;
            record.DischargeDiagnosis = dischargeDiagnosis;
            _inpatientRepo.Update(record);
            await _unitOfWork.SaveChangesAsync();

            return (true, "出院结算完成");
        }

        private static InpatientRecordDto MapToDto(InpatientRecord r) => new()
        {
            Id = r.Id,
            InpatientNo = r.InpatientNo,
            PatientId = r.PatientId, PatientName = r.Patient?.Name,
            BedId = r.BedId, BedNo = r.Bed?.BedNo,
            DepartmentId = r.DepartmentId, DepartmentName = r.Department?.DeptName,
            DoctorId = r.DoctorId, DoctorName = r.Doctor?.Name,
            AdmissionTime = r.AdmissionTime, DischargeTime = r.DischargeTime,
            AdmissionDiagnosis = r.AdmissionDiagnosis, DischargeDiagnosis = r.DischargeDiagnosis,
            Status = r.Status, DepositAmount = r.DepositAmount, TotalCost = r.TotalCost,
            CreateTime = r.CreateTime
        };
    }
}
