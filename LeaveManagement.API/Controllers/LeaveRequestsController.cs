using Microsoft.AspNetCore.Mvc;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                var leaveRequests = await _leaveRequestService.GetLeaveRequestsByEmployeeIdAsync(employeeId);
                return Ok(leaveRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("pending/department-manager/{managerId}")]
        public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPendingRequestsForDepartmentManager(int managerId)
        {
            try
            {
                var leaveRequests = await _leaveRequestService.GetPendingRequestsForDepartmentManagerAsync(managerId);
                return Ok(leaveRequests);
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

                var leaveRequest = await _leaveRequestService.CreateLeaveRequestAsync(createDto);
                return CreatedAtAction(nameof(GetLeaveRequest), new { id = leaveRequest.Id }, leaveRequest);
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

                // Get approver ID from headers or authentication context
                // For now, we'll use a hardcoded value - in real scenario, get from JWT token
                var approverId = 1; // This should come from authentication
                var isHrManager = updateDto.Status == LeaveManagement.Entity.LeaveRequestStatus.ApprovedByHrManager;

                var result = await _leaveRequestService.UpdateLeaveRequestStatusAsync(id, updateDto, approverId, isHrManager);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Leave request status updated successfully" });
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
        public async Task<IActionResult> CancelLeaveRequest(int id, [FromBody] CancelLeaveRequestDto cancelDto)
        {
            try
            {
                var result = await _leaveRequestService.CancelLeaveRequestAsync(id, cancelDto.EmployeeId);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Leave request cancelled successfully" });
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
                var leaveBalances = await _leaveRequestService.GetLeaveBalancesByEmployeeIdAsync(employeeId);
                return Ok(leaveBalances);
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
                var leaveBalances = await _leaveRequestService.GetLeaveBalancesByEmployeeIdAndYearAsync(employeeId, year);
                return Ok(leaveBalances);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class CancelLeaveRequestDto
    {
        public int EmployeeId { get; set; }
    }
}


