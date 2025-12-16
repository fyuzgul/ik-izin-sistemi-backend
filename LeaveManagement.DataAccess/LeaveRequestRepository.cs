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
                    .ThenInclude(e => e.Department)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Include(lr => lr.HrManager)
                .Where(lr => lr.EmployeeId == employeeId)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsForDepartmentManagerAsync(int managerId)
        {
            // Get manager's department
            var context = (Entity.LeaveManagementDbContext)_context;
            var manager = await context.Employees
                .Include(e => e.Department)
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => e.Id == managerId);

            if (manager == null || manager.DepartmentId == null || manager.IsSystemAdmin)
                return new List<LeaveRequest>();

            // Only Yönetici or Direktör can see pending requests
            if (manager.Title == null || 
                (manager.Title.Name != "Yönetici" && manager.Title.Name != "Direktör"))
                return new List<LeaveRequest>();

            // Get pending requests from employees in the same department
            return await _dbSet
                .Include(lr => lr.Employee)
                    .ThenInclude(e => e.Department)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.DepartmentManager)
                .Where(lr => lr.Employee.DepartmentId == manager.DepartmentId && 
                            lr.Status == LeaveRequestStatus.Pending)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsForHrManagerAsync()
        {
            // Get all requests approved by department manager, waiting for HR approval
            return await _dbSet
                .Include(lr => lr.Employee)
                    .ThenInclude(e => e.Department)
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
                    .ThenInclude(e => e.Department)
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
                    .ThenInclude(e => e.Department)
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
                    .ThenInclude(e => e.Department)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Status == status)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(lr => lr.Employee)
                    .ThenInclude(e => e.Department)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.StartDate <= endDate && lr.EndDate >= startDate)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();
        }
    }
}


