using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.API.Extensions;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly LeaveManagementDbContext _context;

        public EmployeesController(IEmployeeService employeeService, LeaveManagementDbContext context)
        {
            _employeeService = employeeService;
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
            
            var hrDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => 
                    EF.Functions.ILike(d.Name, "%İnsan Kaynakları%") || 
                    EF.Functions.ILike(d.Name, "%Human Resources%"));
            
            return hrDepartment != null && hrDepartment.ManagerId == employeeId.Value;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
        {
            try
            {
                // Only Admin and HR Manager can see all employees
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü tüm çalışanları görebilir");
                
                var employees = await _employeeService.GetAllEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                    return NotFound();

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesByDepartment(int departmentId)
        {
            try
            {
                var employees = await _employeeService.GetEmployeesByDepartmentIdAsync(departmentId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("subordinates/{managerId}")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetSubordinates(int managerId)
        {
            try
            {
                var employees = await _employeeService.GetSubordinatesAsync(managerId);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] EmployeeDto employeeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var employee = await _employeeService.CreateEmployeeAsync(employeeDto);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDto employeeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _employeeService.UpdateEmployeeAsync(id, employeeDto);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Employee updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var result = await _employeeService.DeleteEmployeeAsync(id);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            try
            {
                var result = await _employeeService.DeactivateEmployeeAsync(id);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Employee deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateEmployee(int id)
        {
            try
            {
                var result = await _employeeService.ActivateEmployeeAsync(id);
                
                if (!result)
                    return NotFound();

                return Ok(new { message = "Employee activated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


