using Microsoft.EntityFrameworkCore;
using LeaveManagement.Entity;

namespace LeaveManagement.DataAccess
{
    public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(Entity.LeaveManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Include(lr => lr.HrManager)
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsForDepartmentManagerAsync(int managerId)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.DepartmentManagerId == managerId && 
                            lr.Status == LeaveRequestStatus.Pending)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsForHrManagerAsync()
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Where(lr => lr.Status == LeaveRequestStatus.ApprovedByDepartmentManager)
                .OrderByDescending(lr => lr.DepartmentManagerApprovalDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsWithDetailsAsync()
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Include(lr => lr.HrManager)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetLeaveRequestWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Include(lr => lr.HrManager)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<bool> HasOverlappingLeaveRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null)
        {
            var query = _dbSet.Where(lr => lr.EmployeeId == employeeId &&
                                         lr.Status != LeaveRequestStatus.RejectedByDepartmentManager &&
                                         lr.Status != LeaveRequestStatus.RejectedByHrManager &&
                                         lr.Status != LeaveRequestStatus.Cancelled &&
                                         ((lr.StartDate <= startDate && lr.EndDate >= startDate) ||
                                          (lr.StartDate <= endDate && lr.EndDate >= endDate) ||
                                          (lr.StartDate >= startDate && lr.EndDate <= endDate)));

            if (excludeId.HasValue)
            {
                query = query.Where(lr => lr.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByStatusAsync(LeaveRequestStatus status)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Status == status)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.StartDate <= endDate && lr.EndDate >= startDate)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }
    }
}


