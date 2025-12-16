using LeaveManagement.Business.Models;

namespace LeaveManagement.Business.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync();
        Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id);
        Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsForDepartmentManagerAsync(int managerId);
        Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsForHrManagerAsync(int? hrManagerId = null);
        Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestDto createDto);
        Task<bool> UpdateLeaveRequestStatusAsync(int id, UpdateLeaveRequestStatusDto updateDto, int approverId, bool isHrManager = false);
        Task<bool> CancelLeaveRequestAsync(int id, int employeeId);
        Task<bool> DeleteLeaveRequestAsync(int id);
        Task<bool> HasOverlappingLeaveRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null);
        Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesByEmployeeIdAndYearAsync(int employeeId, int year);
    }
}


