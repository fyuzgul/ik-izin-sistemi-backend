using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LeaveManagement.API.Extensions;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-balances")]
    [Authorize]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LeaveManagementDbContext _context;

        public LeaveBalancesController(IUnitOfWork unitOfWork, LeaveManagementDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        private async Task<bool> IsAdminOrHrManagerAsync()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = username == "admin" || role == "Admin";
            
            if (isAdmin) return true;
            
            // Check if user is HR Manager (İnsan Kaynakları departmanının yöneticisi)
            var employeeId = User.GetEmployeeId();
            if (!employeeId.HasValue) return false;
            
            // Get current employee with department and title
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Title)
                .FirstOrDefaultAsync(e => e.Id == employeeId.Value);
            
            if (employee == null || employee.Department == null || employee.Title == null)
                return false;
            
            // Check if employee is in HR department
            var deptName = employee.Department.Name;
            var isHrDept = EF.Functions.ILike(deptName, "%İnsan Kaynakları%") || 
                          EF.Functions.ILike(deptName, "%Human Resources%");
            
            if (!isHrDept) return false;
            
            // Check if employee has manager title (Yönetici or Direktör)
            var isManagerTitle = employee.Title.Name == "Yönetici" || employee.Title.Name == "Direktör";
            
            // If department has ManagerId set, check if it matches
            // Otherwise, if employee is in HR dept with manager title, they are HR Manager
            if (employee.Department.ManagerId.HasValue)
            {
                return employee.Department.ManagerId.Value == employeeId.Value;
            }
            else
            {
                // If ManagerId is not set, but employee is in HR dept with manager title, they are HR Manager
                return isManagerTitle;
            }
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
        public async Task<ActionResult<LeaveBalanceDto>> Create([FromBody] CreateLeaveBalanceDto createDto)
        {
            try
            {
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü izin hakkı tanımlayabilir");
                
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveBalanceDto updateDto)
        {
            try
            {
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü izin hakkı güncelleyebilir");
                
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü izin hakkı silebilir");
                
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
        public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetAll()
        {
            try
            {
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü tüm izin haklarını görebilir");
                
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
