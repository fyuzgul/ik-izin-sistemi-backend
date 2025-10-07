using LeaveManagement.Entity;

namespace LeaveManagement.DataAccess
{
    public interface ILeaveRequestRepository : IRepository<LeaveRequest>
    {
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingRequestsForDepartmentManagerAsync(int managerId);
        Task<IEnumerable<LeaveRequest>> GetPendingRequestsForHrManagerAsync();
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsWithDetailsAsync();
        Task<LeaveRequest?> GetLeaveRequestWithDetailsAsync(int id);
        Task<bool> HasOverlappingLeaveRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(LeaveRequestStatus status);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}


