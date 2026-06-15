using HIS.Common.Enums;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 医嘱服务实现
    /// 核心业务：下达医嘱、护士执行、完成/停止
    /// </summary>
    public class MedicalOrderService : IMedicalOrderService
    {
        private readonly IMedicalOrderRepository _orderRepo;
        private readonly IInpatientRecordRepository _inpatientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly ISysUserRepository _userRepo;
        private readonly IUnitOfWork _unitOfWork;

        public MedicalOrderService(
            IMedicalOrderRepository orderRepo, IInpatientRecordRepository inpatientRepo,
            IDoctorRepository doctorRepo, ISysUserRepository userRepo, IUnitOfWork unitOfWork)
        {
            _orderRepo = orderRepo;
            _inpatientRepo = inpatientRepo;
            _doctorRepo = doctorRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<MedicalOrderDto> List, int Total)> GetListAsync(MedicalOrderQueryModel query)
        {
            var (list, total) = await _orderRepo.GetListAsync(query);
            return (list.Select(MapToDto).ToList(), total);
        }

        public async Task<List<MedicalOrderDto>> GetByInpatientIdAsync(long inpatientId)
        {
            var orders = await _orderRepo.GetByInpatientIdAsync(inpatientId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<MedicalOrderDto?> GetByIdAsync(long id)
        {
            var order = await _orderRepo.GetDetailAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<(bool Success, string Message)> CreateAsync(MedicalOrderDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            var inpatient = await _inpatientRepo.GetByIdAsync(dto.InpatientId);
            if (inpatient == null) return (false, "住院记录不存在");
            if (inpatient.Status != (int)InpatientStatusEnum.InHospital)
                return (false, "患者已出院，不能下达医嘱");

            var doctor = await _doctorRepo.GetByUserIdAsync(userId);
            if (doctor == null) return (false, "当前用户不是医生");

            var order = new MedicalOrder
            {
                InpatientId = dto.InpatientId, PatientId = inpatient.PatientId,
                DoctorId = doctor.Id, OrderType = dto.OrderType,
                OrderContent = dto.OrderContent, StartTime = dto.StartTime,
                EndTime = dto.EndTime, Status = (int)MedicalOrderStatusEnum.Issued,
                CreateTime = DateTime.Now
            };
            await _orderRepo.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return (true, "医嘱下达成功");
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(long id, long executorUserId)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return (false, "医嘱不存在");
            if (order.Status != (int)MedicalOrderStatusEnum.Issued)
                return (false, "该医嘱当前状态不允许执行");

            order.Status = (int)MedicalOrderStatusEnum.Executing;
            order.ExecutorId = executorUserId;
            order.ExecuteTime = DateTime.Now;
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return (true, "医嘱执行成功");
        }

        public async Task<(bool Success, string Message)> CompleteAsync(long id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return (false, "医嘱不存在");
            if (order.Status == (int)MedicalOrderStatusEnum.Stopped)
                return (false, "已停止的医嘱不能完成");

            order.Status = (int)MedicalOrderStatusEnum.Completed;
            order.EndTime = DateTime.Now;
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return (true, "医嘱已完成");
        }

        public async Task<(bool Success, string Message)> StopAsync(long id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return (false, "医嘱不存在");
            if (order.Status == (int)MedicalOrderStatusEnum.Completed)
                return (false, "已完成的医嘱不能停止");

            order.Status = (int)MedicalOrderStatusEnum.Stopped;
            order.EndTime = DateTime.Now;
            _orderRepo.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return (true, "医嘱已停止");
        }

        private static MedicalOrderDto MapToDto(MedicalOrder o) => new()
        {
            Id = o.Id, InpatientId = o.InpatientId,
            InpatientNo = o.InpatientRecord?.InpatientNo,
            PatientId = o.PatientId, PatientName = o.Patient?.Name,
            DoctorId = o.DoctorId, DoctorName = o.Doctor?.Name,
            OrderType = o.OrderType, OrderContent = o.OrderContent,
            StartTime = o.StartTime, EndTime = o.EndTime,
            Status = o.Status, ExecutorId = o.ExecutorId, ExecuteTime = o.ExecuteTime,
            CreateTime = o.CreateTime
        };
    }
}
