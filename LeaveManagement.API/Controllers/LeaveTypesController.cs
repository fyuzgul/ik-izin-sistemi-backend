using Microsoft.AspNetCore.Mvc;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-types")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaveTypesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetAllLeaveTypes()
        {
            try
            {
                var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();
                var leaveTypeDtos = leaveTypes.Select(MapToDto);
                return Ok(leaveTypeDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveTypeDto>> GetLeaveType(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
                if (leaveType == null)
                    return NotFound();

                return Ok(MapToDto(leaveType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<LeaveTypeDto>> CreateLeaveType([FromBody] CreateLeaveTypeDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var leaveType = new LeaveType
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    MaxDaysPerYear = createDto.MaxDaysPerYear,
                    RequiresApproval = createDto.RequiresApproval,
                    IsPaid = createDto.IsPaid,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.LeaveTypes.AddAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLeaveType), new { id = leaveType.Id }, MapToDto(leaveType));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLeaveType(int id, [FromBody] UpdateLeaveTypeDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
                if (leaveType == null)
                    return NotFound();

                leaveType.Name = updateDto.Name;
                leaveType.Description = updateDto.Description;
                leaveType.MaxDaysPerYear = updateDto.MaxDaysPerYear;
                leaveType.RequiresApproval = updateDto.RequiresApproval;
                leaveType.IsPaid = updateDto.IsPaid;
                leaveType.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.LeaveTypes.UpdateAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Leave type updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLeaveType(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
                if (leaveType == null)
                    return NotFound();

                // Check if there are any leave requests using this type
                var hasLeaveRequests = await _unitOfWork.LeaveRequests.ExistsAsync(lr => lr.LeaveTypeId == id);
                if (hasLeaveRequests)
                    return BadRequest("Cannot delete leave type that has associated leave requests");

                await _unitOfWork.LeaveTypes.DeleteAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Leave type deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateLeaveType(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
                if (leaveType == null)
                    return NotFound();

                leaveType.IsActive = true;
                leaveType.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.LeaveTypes.UpdateAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Leave type activated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateLeaveType(int id)
        {
            try
            {
                var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
                if (leaveType == null)
                    return NotFound();

                leaveType.IsActive = false;
                leaveType.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.LeaveTypes.UpdateAsync(leaveType);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Leave type deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private LeaveTypeDto MapToDto(LeaveType leaveType)
        {
            return new LeaveTypeDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                Description = leaveType.Description,
                MaxDaysPerYear = leaveType.MaxDaysPerYear,
                RequiresApproval = leaveType.RequiresApproval,
                IsPaid = leaveType.IsPaid,
                IsActive = leaveType.IsActive,
                CreatedDate = leaveType.CreatedDate,
                UpdatedDate = leaveType.UpdatedDate
            };
        }
    }

    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; } = true;
        public bool IsPaid { get; set; } = true;
    }

    public class UpdateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxDaysPerYear { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsPaid { get; set; }
    }
}


