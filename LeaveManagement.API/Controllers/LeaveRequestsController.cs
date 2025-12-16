using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.API.Extensions;
using LeaveManagement.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-requests")]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly Entity.LeaveManagementDbContext _context;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService, Entity.LeaveManagementDbContext context)
        {
            _leaveRequestService = leaveRequestService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetAllLeaveRequests()
        {
            try
            {
                var leaveRequests = await _leaveRequestService.GetAllLeaveRequestsAsync();
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("my-requests")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetMyLeaveRequests()
        {
            try
            {
                // Get employee ID from JWT token
                var employeeId = User.GetEmployeeId();
                if (employeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                var leaveRequests = await _leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId.Value);
                return Ok(leaveRequests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequest(int id)
        {
            try
            {
                var leaveRequest = await _leaveRequestService.GetLeaveRequestByIdAsync(id);
                if (leaveRequest == null)
                    return NotFound();

                return Ok(leaveRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequestsByEmployee(int employeeId)
        {
            try
            {
                // Get employee ID from JWT token
                var currentEmployeeId = User.GetEmployeeId();
                if (currentEmployeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                // Employees can only view their own leave requests
                // Managers and HR can view any employee's requests (we'll check roles)
                var userRole = User.GetUserRole();
                
                if (currentEmployeeId.Value != employeeId && 
                    userRole != "Yönetici" && 
                    userRole != "İK Müdürü" && 
                    userRole != "Admin")
                {
                    return Forbid();
                }

                var leaveRequests = await _leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId);
                return Ok(leaveRequests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("pending/department-manager")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPendingRequestsForDepartmentManager()
        {
            try
            {
                // Get employee ID from JWT token
                var managerId = User.GetEmployeeId();
                if (managerId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                Console.WriteLine($"[DEBUG] Pending requests için managerId: {managerId}");
                
                var leaveRequests = await _leaveRequestService.GetPendingRequestsForDepartmentManagerAsync(managerId.Value);
                
                Console.WriteLine($"[DEBUG] Bulunan talep sayısı: {leaveRequests.Count()}");
                foreach (var req in leaveRequests)
                {
                    Console.WriteLine($"[DEBUG] Request ID: {req.Id}, Employee: {req.EmployeeName}, DeptManagerId: {req.DepartmentManagerId}");
                }
                
                return Ok(leaveRequests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("pending/hr-manager")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPendingRequestsForHrManager()
        {
            try
            {
                // Get employee ID from JWT token
                var hrManagerId = User.GetEmployeeId();
                if (hrManagerId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                // Check if user is admin - admin can see all HR approvals
                var isAdmin = User.FindFirst(ClaimTypes.Name)?.Value == "admin" || 
                             User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
                
                if (isAdmin)
                {
                    // Admin can see all HR manager approvals
                    var leaveRequests = await _leaveRequestService.GetPendingRequestsForHrManagerAsync(null);
                    return Ok(leaveRequests);
                }
                
                // Verify that the caller is the HR manager
                var hrDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => 
                        EF.Functions.ILike(d.Name, "%İnsan Kaynakları%") || 
                        EF.Functions.ILike(d.Name, "%Human Resources%"));
                
                if (hrDepartment == null || hrDepartment.ManagerId != hrManagerId.Value)
                {
                    // Caller is not the HR manager, return empty list
                    return Ok(new List<LeaveRequestDto>());
                }
                
                var hrLeaveRequests = await _leaveRequestService.GetPendingRequestsForHrManagerAsync(hrManagerId.Value);
                return Ok(hrLeaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest([FromBody] CreateLeaveRequestDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Get employee ID from JWT token
                var employeeId = User.GetEmployeeId();
                if (employeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                // Override the employee ID from the request with the one from the token
                // This ensures employees can only create leave requests for themselves
                createDto.EmployeeId = employeeId.Value;

                var leaveRequest = await _leaveRequestService.CreateLeaveRequestAsync(createDto);
                return CreatedAtAction(nameof(GetLeaveRequest), new { id = leaveRequest.Id }, leaveRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] UpdateLeaveRequestStatusDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Get approver ID from JWT token
                var approverId = User.GetEmployeeId();
                if (approverId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                var userTitle = User.GetUserRole(); // This now returns TitleName from JWT
                
                // Determine if this is HR manager approval
                // Check if approver is HR Department Manager (İnsan Kaynakları departmanının ManagerId'si)
                var isHrManager = false;
                
                // Check if status indicates HR approval
                if (updateDto.Status == LeaveManagement.Entity.LeaveRequestStatus.ApprovedByHrManager ||
                    updateDto.Status == LeaveManagement.Entity.LeaveRequestStatus.RejectedByHrManager)
                {
                    // Verify that approver is actually HR Department Manager
                    var hrDepartment = await _context.Departments
                        .FirstOrDefaultAsync(d => d.Name.Contains("İnsan Kaynakları") || d.Name.Contains("Human Resources"));
                    
                    if (hrDepartment != null && hrDepartment.ManagerId == approverId.Value)
                    {
                        isHrManager = true;
                    }
                    else
                    {
                        // Status says HR approval but approver is not HR manager
                        return Unauthorized("Sadece İnsan Kaynakları departman yöneticisi İK onayı yapabilir.");
                    }
                }

                var result = await _leaveRequestService.UpdateLeaveRequestStatusAsync(id, updateDto, approverId.Value, isHrManager);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "İzin talebi durumu başarıyla güncellendi" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int id)
        {
            try
            {
                // Get employee ID from JWT token
                var employeeId = User.GetEmployeeId();
                if (employeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                var result = await _leaveRequestService.CancelLeaveRequestAsync(id, employeeId.Value);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "İzin talebi başarıyla iptal edildi" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveRequest(int id)
        {
            try
            {
                var result = await _leaveRequestService.DeleteLeaveRequestAsync(id);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Leave request deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employee/{employeeId}/balance")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(int employeeId)
        {
            try
            {
                // Get employee ID from JWT token
                var currentEmployeeId = User.GetEmployeeId();
                if (currentEmployeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                var userRole = User.GetUserRole();
                
                // Employees can only view their own balance
                // Managers and HR can view any employee's balance
                if (currentEmployeeId.Value != employeeId && 
                    userRole != "Yönetici" && 
                    userRole != "İK Müdürü" && 
                    userRole != "Admin")
                {
                    return Forbid();
                }

                var leaveBalances = await _leaveRequestService.GetLeaveBalancesByEmployeeIdAsync(employeeId);
                return Ok(leaveBalances);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employee/{employeeId}/balance/{year}")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalanceByYear(int employeeId, int year)
        {
            try
            {
                // Get employee ID from JWT token
                var currentEmployeeId = User.GetEmployeeId();
                if (currentEmployeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                
                var userRole = User.GetUserRole();
                
                // Employees can only view their own balance
                // Managers and HR can view any employee's balance
                if (currentEmployeeId.Value != employeeId && 
                    userRole != "Yönetici" && 
                    userRole != "İK Müdürü" && 
                    userRole != "Admin")
                {
                    return Forbid();
                }

                var leaveBalances = await _leaveRequestService.GetLeaveBalancesByEmployeeIdAndYearAsync(employeeId, year);
                return Ok(leaveBalances);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


