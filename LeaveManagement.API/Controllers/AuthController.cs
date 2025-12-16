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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly LeaveManagementDbContext _context;

        public AuthController(IAuthService authService, LeaveManagementDbContext context)
        {
            _authService = authService;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            try
            {
                var result = await _authService.CreateEmployeeAsync(createEmployeeDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Çalışan oluşturulamadı. Kullanıcı adı veya çalışan numarası zaten kullanımda." });
                }

                return CreatedAtAction(nameof(GetEmployee), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("employees")]
        [Authorize]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                // Only Admin and HR Manager can see all employees
                if (!await IsAdminOrHrManagerAsync())
                    return Forbid("Sadece sistem admin veya İK Müdürü tüm çalışanları görebilir");
                
                var employees = await _authService.GetAllEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("employees/{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            try
            {
                var employee = await _authService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound(new { message = "Çalışan bulunamadı" });
                }

                return Ok(employee);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("employees/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                var result = await _authService.UpdateEmployeeAsync(id, updateEmployeeDto);
                if (!result)
                {
                    return BadRequest(new { message = "Çalışan güncellenemedi" });
                }

                return Ok(new { message = "Çalışan başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("employees/{id}")]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            try
            {
                var result = await _authService.DeactivateEmployeeAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Çalışan bulunamadı" });
                }

                return Ok(new { message = "Çalışan başarıyla deaktive edildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("titles")]
        public async Task<IActionResult> GetTitles()
        {
            try
            {
                var titles = await _authService.GetTitlesAsync();
                return Ok(titles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin()
        {
            try
            {
                // Check if admin already exists
                var adminExists = await _authService.GetAllEmployeesAsync();
                if (adminExists.Any(e => e.Username == "admin"))
                {
                    return BadRequest(new { message = "Admin kullanıcısı zaten mevcut" });
                }

                // Create admin employee
                var adminDto = new CreateEmployeeDto
                {
                    FirstName = "Sistem",
                    LastName = "Admin",
                    Email = "admin@company.com",
                    EmployeeNumber = "SYS001",
                    Username = "admin",
                    Password = "Admin123!",
                    TitleId = 1, // Yönetici
                    DepartmentId = 1, // İnsan Kaynakları (should exist from DbInitializer)
                    IsActive = true,
                    HireDate = DateTime.UtcNow
                };

                var result = await _authService.CreateEmployeeAsync(adminDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Admin kullanıcısı oluşturulamadı" });
                }

                return Ok(new { message = "Admin kullanıcısı başarıyla oluşturuldu", employee = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }
}