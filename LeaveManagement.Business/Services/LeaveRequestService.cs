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

        public LeaveRequestService(IUnitOfWork unitOfWork, ILeaveRequestRepository leaveRequestRepository)
        {
            _unitOfWork = unitOfWork;
            _leaveRequestRepository = leaveRequestRepository;
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

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsForHrManagerAsync()
        {
            var leaveRequests = await _leaveRequestRepository.GetPendingRequestsForHrManagerAsync();
            return leaveRequests.Select(MapToDto);
        }

        public async Task<LeaveRequestDto> CreateLeaveRequestAsync(CreateLeaveRequestDto createDto)
        {
            // Validate employee exists
            var employee = await _unitOfWork.Employees.GetByIdAsync(createDto.EmployeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            // Validate leave type exists
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(createDto.LeaveTypeId);
            if (leaveType == null)
                throw new ArgumentException("Leave type not found");

            // Check for overlapping requests
            var hasOverlapping = await HasOverlappingLeaveRequestAsync(createDto.EmployeeId, createDto.StartDate, createDto.EndDate);
            if (hasOverlapping)
                throw new InvalidOperationException("Overlapping leave request exists");

            // Check leave balance
            var currentYear = DateTime.Now.Year;
            var leaveBalance = await _unitOfWork.LeaveBalances.FirstOrDefaultAsync(lb => 
                lb.EmployeeId == createDto.EmployeeId && 
                lb.LeaveTypeId == createDto.LeaveTypeId && 
                lb.Year == currentYear);

            var totalDays = (createDto.EndDate - createDto.StartDate).Days + 1;
            if (leaveBalance == null || leaveBalance.RemainingDays < totalDays)
                throw new InvalidOperationException("Insufficient leave balance");

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
                DepartmentManagerId = employee.ManagerId,
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

            // Validate approval flow
            if (isHrManager)
            {
                if (leaveRequest.Status != LeaveRequestStatus.ApprovedByDepartmentManager)
                    throw new InvalidOperationException("Leave request must be approved by department manager first");
                
                leaveRequest.HrManagerId = approverId;
                leaveRequest.Status = updateDto.Status == LeaveRequestStatus.ApprovedByHrManager 
                    ? LeaveRequestStatus.ApprovedByHrManager 
                    : LeaveRequestStatus.RejectedByHrManager;
                leaveRequest.HrManagerApprovalDate = DateTime.UtcNow;
                leaveRequest.HrManagerComments = updateDto.Comments;

                // If approved by HR, update leave balance
                if (updateDto.Status == LeaveRequestStatus.ApprovedByHrManager)
                {
                    await UpdateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, leaveRequest.TotalDays);
                }
            }
            else
            {
                if (leaveRequest.Status != LeaveRequestStatus.Pending)
                    throw new InvalidOperationException("Leave request is not in pending status");

                if (leaveRequest.DepartmentManagerId != approverId)
                    throw new UnauthorizedAccessException("Only department manager can approve this request");

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
            // This is a simple implementation - in real scenario, you might have a specific HR department or role
            var hrDepartment = await _unitOfWork.Departments.FirstOrDefaultAsync(d => d.Name.Contains("Human Resources"));
            return hrDepartment?.ManagerId;
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


