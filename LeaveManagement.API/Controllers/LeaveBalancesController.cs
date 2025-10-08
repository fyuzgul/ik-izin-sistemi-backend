using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-balances")]
    [Authorize]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaveBalancesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetByEmployee(int employeeId)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var balances = await _unitOfWork.LeaveBalances
                    .FindAsync(lb => lb.EmployeeId == employeeId && lb.Year == currentYear);

                var result = new List<LeaveBalanceDto>();
                foreach (var balance in balances)
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("employee/{employeeId}/year/{year}")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetByEmployeeAndYear(int employeeId, int year)
        {
            try
            {
                var balances = await _unitOfWork.LeaveBalances
                    .FindAsync(lb => lb.EmployeeId == employeeId && lb.Year == year);

                var result = new List<LeaveBalanceDto>();
                foreach (var balance in balances)
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,İK Müdürü")]
        public async Task<ActionResult<LeaveBalanceDto>> Create([FromBody] CreateLeaveBalanceDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if balance already exists
                var existing = await _unitOfWork.LeaveBalances
                    .FirstOrDefaultAsync(lb => 
                        lb.EmployeeId == createDto.EmployeeId && 
                        lb.LeaveTypeId == createDto.LeaveTypeId && 
                        lb.Year == createDto.Year);

                if (existing != null)
                    return BadRequest("Bu çalışan için bu yıl ve izin türünde zaten hak tanımlanmış");

                var leaveBalance = new LeaveBalance
                {
                    EmployeeId = createDto.EmployeeId,
                    LeaveTypeId = createDto.LeaveTypeId,
                    Year = createDto.Year,
                    TotalDays = createDto.TotalDays,
                    UsedDays = 0,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.LeaveBalances.AddAsync(leaveBalance);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "İzin hakkı başarıyla tanımlandı", id = leaveBalance.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,İK Müdürü")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveBalanceDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var leaveBalance = await _unitOfWork.LeaveBalances.GetByIdAsync(id);
                if (leaveBalance == null)
                    return NotFound("İzin hakkı bulunamadı");

                leaveBalance.TotalDays = updateDto.TotalDays;
                leaveBalance.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.LeaveBalances.UpdateAsync(leaveBalance);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "İzin hakkı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,İK Müdürü")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var leaveBalance = await _unitOfWork.LeaveBalances.GetByIdAsync(id);
                if (leaveBalance == null)
                    return NotFound("İzin hakkı bulunamadı");

                // Check if any days have been used
                if (leaveBalance.UsedDays > 0)
                    return BadRequest("Kullanılmış izin hakkı silinemez");

                await _unitOfWork.LeaveBalances.DeleteAsync(leaveBalance);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "İzin hakkı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,İK Müdürü")]
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetAll()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var balances = await _unitOfWork.LeaveBalances
                    .FindAsync(lb => lb.Year == currentYear);

                var result = new List<LeaveBalanceDto>();
                foreach (var balance in balances)
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class LeaveBalanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
    }

    public class CreateLeaveBalanceDto
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public int Year { get; set; }
        public int TotalDays { get; set; }
    }

    public class UpdateLeaveBalanceDto
    {
        public int TotalDays { get; set; }
    }
}
