using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Business.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly LeaveManagementDbContext _context;

        public LeaveRequestService(IUnitOfWork unitOfWork, ILeaveRequestRepository leaveRequestRepository, LeaveManagementDbContext context)
        {
            _unitOfWork = unitOfWork;
            _leaveRequestRepository = leaveRequestRepository;
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync()
        {
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetailsAsync();
            return leaveRequests.Select(MapToDto);
        }

        public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id)
        {
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestWithDetailsAsync(id);
            return leaveRequest != null ? MapToDto(leaveRequest) : null;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsByEmployeeIdAsync(employeeId);
            return leaveRequests.Select(MapToDto);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsForDepartmentManagerAsync(int managerId)
        {
            var leaveRequests = await _leaveRequestRepository.GetPendingRequestsForDepartmentManagerAsync(managerId);
            return leaveRequests.Select(MapToDto);
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsForHrManagerAsync(int? hrManagerId = null)
        {
            var leaveRequests = await _leaveRequestRepository.GetPendingRequestsForHrManagerAsync();
            
            // If hrManagerId is provided, verify that the caller is the HR manager
            if (hrManagerId.HasValue)
            {
                var hrManagerIdFromDb = await GetHrManagerIdAsync();
                if (hrManagerIdFromDb != hrManagerId.Value)
                {
                    // Caller is not the HR manager, return empty list
                    return new List<LeaveRequestDto>();
                }
            }
            
            return leaveRequests.Select(MapToDto);
        }

        public async Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestDto createDto)
        {
            // Validate employee exists and get fresh data from database
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => e.Id == createDto.EmployeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");
            
            // Find department manager (Yönetici or Direktör in the same department)
            var departmentManager = await FindDepartmentManagerAsync(employee.DepartmentId);
            if (departmentManager == null)
                throw new InvalidOperationException("Departmanınızda onay yetkisine sahip bir yönetici bulunamadı. Lütfen İK departmanı ile iletişime geçin.");

            // Validate leave type exists
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(createDto.LeaveTypeId);
            if (leaveType == null)
                throw new ArgumentException("Leave type not found");

            // Check for overlapping requests
            var hasOverlapping = await HasOverlappingLeaveRequestAsync(createDto.EmployeeId, createDto.StartDate, createDto.EndDate);
            if (hasOverlapping)
                throw new InvalidOperationException("Overlapping leave request exists");

            // Calculate total days excluding weekends and holidays based on employee's work schedule
            var totalDays = await CalculateWorkingDaysAsync(createDto.StartDate, createDto.EndDate, employee.WorksOnSaturday);
            var currentYear = DateTime.Now.Year;

            // Check leave balance if required for this leave type
            if (leaveType.RequiresBalance)
            {
                var leaveBalance = await _unitOfWork.LeaveBalances.FirstOrDefaultAsync(lb => 
                    lb.EmployeeId == createDto.EmployeeId && 
                    lb.LeaveTypeId == createDto.LeaveTypeId && 
                    lb.Year == currentYear);

                if (leaveBalance == null || leaveBalance.RemainingDays < totalDays)
                    throw new InvalidOperationException("Yetersiz izin bakiyesi. Bu izin türü için yeterli bakiyeniz bulunmamaktadır.");
            }
            else
            {
                // For leave types that don't require balance (Ücretsiz İzin)
                // Check if employee has annual leave balance - if yes, they cannot request unpaid leave
                var isUnpaidLeave = leaveType.Name == "Ücretsiz İzin";
                if (isUnpaidLeave)
                {
                    var annualLeaveType = await _unitOfWork.LeaveTypes.FirstOrDefaultAsync(lt => lt.Name == "Yıllık İzin");
                    if (annualLeaveType != null)
                    {
                        var annualLeaveBalance = await _unitOfWork.LeaveBalances.FirstOrDefaultAsync(lb => 
                            lb.EmployeeId == createDto.EmployeeId && 
                            lb.LeaveTypeId == annualLeaveType.Id && 
                            lb.Year == currentYear);

                        if (annualLeaveBalance != null && annualLeaveBalance.RemainingDays > 0)
                            throw new InvalidOperationException("Yıllık izin hakkınız bulunduğu için ücretsiz izin talep edemezsiniz.");
                    }
                }
            }

            // Create leave request
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = createDto.EmployeeId,
                LeaveTypeId = createDto.LeaveTypeId,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                TotalDays = totalDays,
                Reason = createDto.Reason,
                Status = LeaveRequestStatus.Pending,
                DepartmentManagerId = departmentManager.Id,
                HrManagerId = await GetHrManagerIdAsync()
            };

            await _unitOfWork.LeaveRequests.AddAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            // Get the created leave request with details
            var createdRequest = await _leaveRequestRepository.GetLeaveRequestWithDetailsAsync(leaveRequest.Id);
            return MapToDto(createdRequest!);
        }

        public async Task<bool> UpdateLeaveRequestStatusAsync(int id, UpdateLeaveRequestStatusDto updateDto, int approverId, bool isHrManager = false)
        {
            var leaveRequest = await _leaveRequestRepository.GetLeaveRequestWithDetailsAsync(id);
            if (leaveRequest == null)
                return false;

            // Get approver employee with title
            var approver = await _context.Employees
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => e.Id == approverId);
            
            if (approver == null || approver.Title == null)
                throw new UnauthorizedAccessException("Onay yetkiniz bulunmamaktadır.");

            // Validate approval flow
            if (isHrManager)
            {
                // HR Manager must be Direktör or Yönetici
                if (approver.Title.Name != "Direktör" && approver.Title.Name != "Yönetici")
                    throw new UnauthorizedAccessException("Sadece Yönetici veya Direktör ünvanına sahip kişiler onaylayabilir.");

                if (leaveRequest.Status != LeaveRequestStatus.ApprovedByDepartmentManager)
                    throw new InvalidOperationException("İzin talebi önce departman yöneticisi tarafından onaylanmalıdır.");
                
                leaveRequest.HrManagerId = approverId;
                leaveRequest.Status = updateDto.Status == LeaveRequestStatus.ApprovedByHrManager 
                    ? LeaveRequestStatus.ApprovedByHrManager 
                    : LeaveRequestStatus.RejectedByHrManager;
                leaveRequest.HrManagerApprovalDate = DateTime.UtcNow;
                leaveRequest.HrManagerComments = updateDto.Comments;

                // If approved by HR, update leave balance only for leave types that deduct from balance
                if (updateDto.Status == LeaveRequestStatus.ApprovedByHrManager)
                {
                    // Check if this leave type deducts from balance
                    if (leaveRequest.LeaveType.DeductsFromBalance)
                    {
                        await UpdateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, leaveRequest.TotalDays);
                    }
                }
            }
            else
            {
                // Department Manager must be Yönetici or Direktör
                if (approver.Title.Name != "Yönetici" && approver.Title.Name != "Direktör")
                    throw new UnauthorizedAccessException("Sadece Yönetici veya Direktör ünvanına sahip kişiler onaylayabilir.");

                if (leaveRequest.Status != LeaveRequestStatus.Pending)
                    throw new InvalidOperationException("İzin talebi bekleyen durumda değil.");

                // Check if approver is the department manager of the request
                if (leaveRequest.DepartmentManagerId != approverId)
                    throw new UnauthorizedAccessException("Bu izin talebini sadece departman yöneticisi onaylayabilir.");

                // Verify approver is in the same department as the employee
                if (approver.DepartmentId != leaveRequest.Employee.DepartmentId)
                    throw new UnauthorizedAccessException("Sadece aynı departmandaki yönetici onaylayabilir.");

                leaveRequest.Status = updateDto.Status == LeaveRequestStatus.ApprovedByDepartmentManager 
                    ? LeaveRequestStatus.ApprovedByDepartmentManager 
                    : LeaveRequestStatus.RejectedByDepartmentManager;
                leaveRequest.DepartmentManagerApprovalDate = DateTime.UtcNow;
                leaveRequest.DepartmentManagerComments = updateDto.Comments;
            }

            leaveRequest.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelLeaveRequestAsync(int id, int employeeId)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(id);
            if (leaveRequest == null || leaveRequest.EmployeeId != employeeId)
                return false;

            if (leaveRequest.Status != LeaveRequestStatus.Pending && 
                leaveRequest.Status != LeaveRequestStatus.ApprovedByDepartmentManager)
                throw new InvalidOperationException("Cannot cancel leave request in current status");

            leaveRequest.Status = LeaveRequestStatus.Cancelled;
            leaveRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveRequests.UpdateAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteLeaveRequestAsync(int id)
        {
            var leaveRequest = await _unitOfWork.LeaveRequests.GetByIdAsync(id);
            if (leaveRequest == null)
                return false;

            if (leaveRequest.Status == LeaveRequestStatus.ApprovedByHrManager)
                throw new InvalidOperationException("Cannot delete approved leave request");

            await _unitOfWork.LeaveRequests.DeleteAsync(leaveRequest);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HasOverlappingLeaveRequestAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeId = null)
        {
            return await _leaveRequestRepository.HasOverlappingLeaveRequestAsync(employeeId, startDate, endDate, excludeId);
        }

        public async Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesByEmployeeIdAsync(int employeeId)
        {
            var currentYear = DateTime.Now.Year;
            return await GetLeaveBalancesByEmployeeIdAndYearAsync(employeeId, currentYear);
        }

        public async Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesByEmployeeIdAndYearAsync(int employeeId, int year)
        {
            var leaveBalances = await _unitOfWork.LeaveBalances.FindAsync(lb => 
                lb.EmployeeId == employeeId && lb.Year == year);

            var result = new List<LeaveBalanceDto>();

            foreach (var balance in leaveBalances)
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(balance.EmployeeId);
                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(balance.LeaveTypeId);

                result.Add(new LeaveBalanceDto
                {
                    Id = balance.Id,
                    EmployeeId = balance.EmployeeId,
                    EmployeeName = $"{employee!.FirstName} {employee.LastName}",
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveTypeName = leaveType!.Name,
                    Year = balance.Year,
                    TotalDays = balance.TotalDays,
                    UsedDays = balance.UsedDays,
                    RemainingDays = balance.RemainingDays
                });
            }

            return result;
        }

        private async Task<int?> GetHrManagerIdAsync()
        {
            // Find HR Manager - İnsan Kaynakları departmanının ManagerId'sine sahip kişi
            // Bu kişi departman yöneticisi olarak atanmış olmalı
            var hrDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name.Contains("İnsan Kaynakları") || d.Name.Contains("Human Resources"));
            
            if (hrDepartment == null || hrDepartment.ManagerId == null)
                return null;

            // Get the HR Department Manager (exclude system admin)
            var hrManager = await _context.Employees
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => 
                    e.Id == hrDepartment.ManagerId.Value && 
                    e.IsActive &&
                    !e.IsSystemAdmin && // Exclude system admin
                    e.Title != null && 
                    (e.Title.Name == "Direktör" || e.Title.Name == "Yönetici"));

            return hrManager?.Id;
        }

        private async Task<Employee?> FindDepartmentManagerAsync(int? departmentId)
        {
            if (departmentId == null)
                return null;

            // Find Yönetici or Direktör in the same department (exclude system admin)
            var manager = await _context.Employees
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => 
                    e.DepartmentId == departmentId && 
                    e.IsActive &&
                    !e.IsSystemAdmin && // Exclude system admin
                    e.Title != null && 
                    (e.Title.Name == "Yönetici" || e.Title.Name == "Direktör"));

            return manager;
        }

        private async Task<int> CalculateWorkingDaysAsync(DateTime startDate, DateTime endDate, bool worksOnSaturday)
        {
            // Get all active holidays for the date range
            var holidays = await _context.Holidays
                .Where(h => h.IsActive && 
                           h.Date.Date >= startDate.Date && 
                           h.Date.Date <= endDate.Date)
                .Select(h => h.Date.Date)
                .ToListAsync();

            int workingDays = 0;
            DateTime currentDate = startDate.Date;
            DateTime end = endDate.Date;

            while (currentDate <= end)
            {
                // Sunday is always a non-working day
                if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Saturday is a non-working day only if employee doesn't work on Saturdays
                if (currentDate.DayOfWeek == DayOfWeek.Saturday && !worksOnSaturday)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Check if current date is a holiday
                if (holidays.Contains(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                workingDays++;
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }

        private async Task UpdateLeaveBalanceAsync(int employeeId, int leaveTypeId, int usedDays)
        {
            var currentYear = DateTime.Now.Year;
            var leaveBalance = await _unitOfWork.LeaveBalances.FirstOrDefaultAsync(lb => 
                lb.EmployeeId == employeeId && 
                lb.LeaveTypeId == leaveTypeId && 
                lb.Year == currentYear);

            if (leaveBalance != null)
            {
                leaveBalance.UsedDays += usedDays;
                leaveBalance.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.LeaveBalances.UpdateAsync(leaveBalance);
            }
        }

        private LeaveRequestDto MapToDto(LeaveRequest leaveRequest)
        {
            return new LeaveRequestDto
            {
                Id = leaveRequest.Id,
                EmployeeId = leaveRequest.EmployeeId,
                EmployeeName = $"{leaveRequest.Employee.FirstName} {leaveRequest.Employee.LastName}",
                LeaveTypeId = leaveRequest.LeaveTypeId,
                LeaveTypeName = leaveRequest.LeaveType.Name,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                TotalDays = leaveRequest.TotalDays,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                DepartmentManagerId = leaveRequest.DepartmentManagerId,
                DepartmentManagerName = leaveRequest.DepartmentManager != null 
                    ? $"{leaveRequest.DepartmentManager.FirstName} {leaveRequest.DepartmentManager.LastName}" 
                    : null,
                DepartmentName = leaveRequest.Employee.Department != null 
                    ? leaveRequest.Employee.Department.Name 
                    : null,
                HrManagerId = leaveRequest.HrManagerId,
                HrManagerName = leaveRequest.HrManager != null 
                    ? $"{leaveRequest.HrManager.FirstName} {leaveRequest.HrManager.LastName}" 
                    : null,
                DepartmentManagerApprovalDate = leaveRequest.DepartmentManagerApprovalDate,
                HrManagerApprovalDate = leaveRequest.HrManagerApprovalDate,
                DepartmentManagerComments = leaveRequest.DepartmentManagerComments,
                HrManagerComments = leaveRequest.HrManagerComments,
                CreatedDate = leaveRequest.CreatedDate,
                UpdatedDate = leaveRequest.UpdatedDate
            };
        }
    }
}


