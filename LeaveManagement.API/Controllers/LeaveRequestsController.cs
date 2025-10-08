using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.API.Extensions;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-requests")]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
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
                
                // Employees can only view their own leave requests
                // Managers and HR can view any employee's requests (we'll check roles)
                var userRole = User.GetUserRole();
                
                if (currentEmployeeId != employeeId && 
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
                
                Console.WriteLine($"[DEBUG] Pending requests için managerId: {managerId}");
                
                var leaveRequests = await _leaveRequestService.GetPendingRequestsForDepartmentManagerAsync(managerId);
                
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
                var leaveRequests = await _leaveRequestService.GetPendingRequestsForHrManagerAsync();
                return Ok(leaveRequests);
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
                
                // Override the employee ID from the request with the one from the token
                // This ensures employees can only create leave requests for themselves
                createDto.EmployeeId = employeeId;

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
                var userRole = User.GetUserRole();
                
                // Determine if this is HR manager approval
                var isHrManager = userRole == "İK Müdürü" || 
                                 updateDto.Status == LeaveManagement.Entity.LeaveRequestStatus.ApprovedByHrManager ||
                                 updateDto.Status == LeaveManagement.Entity.LeaveRequestStatus.RejectedByHrManager;

                var result = await _leaveRequestService.UpdateLeaveRequestStatusAsync(id, updateDto, approverId, isHrManager);
                
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
                
                var result = await _leaveRequestService.CancelLeaveRequestAsync(id, employeeId);
                
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
                var userRole = User.GetUserRole();
                
                // Employees can only view their own balance
                // Managers and HR can view any employee's balance
                if (currentEmployeeId != employeeId && 
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
                var userRole = User.GetUserRole();
                
                // Employees can only view their own balance
                // Managers and HR can view any employee's balance
                if (currentEmployeeId != employeeId && 
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


