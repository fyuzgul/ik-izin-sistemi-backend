using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace LeaveManagement.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly LeaveManagementDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(LeaveManagementDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateJwtToken(Employee employee)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Username ?? ""),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Role, employee.Role?.Name ?? ""),
                new Claim("RoleId", employee.RoleId?.ToString() ?? "0"),
                new Claim("EmployeeId", employee.Id.ToString()),
                new Claim("DepartmentId", employee.DepartmentId?.ToString() ?? "0"),
                new Claim("DepartmentName", employee.Department?.Name ?? ""),
                new Claim("EmployeeName", $"{employee.FirstName} {employee.LastName}"),
                new Claim("IsActive", employee.IsActive.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "LeaveManagement",
                audience: _configuration["Jwt:Audience"] ?? "LeaveManagement",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Username == loginDto.Username && e.IsActive);

            if (employee == null || employee.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, employee.PasswordHash))
            {
                return null;
            }

            // Update last login date
            employee.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(employee);

            return new LoginResponseDto
            {
                Token = token
            };
        }

        public async Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            // Check if username already exists
            if (!string.IsNullOrEmpty(createEmployeeDto.Username))
            {
                var existingEmployee = await _context.Employees
                    .AnyAsync(e => e.Username == createEmployeeDto.Username);

                if (existingEmployee)
                {
                    return null;
                }
            }

            // Check if employee number already exists
            var existingEmployeeNumber = await _context.Employees
                .AnyAsync(e => e.EmployeeNumber == createEmployeeDto.EmployeeNumber);

            if (existingEmployeeNumber)
            {
                return null;
            }

            // Get role to check if this is a manager
            var role = await _context.Roles.FindAsync(createEmployeeDto.RoleId);
            var isManager = role != null && (role.Name == "Yönetici" || role.Name == "İK Müdürü" || role.Name == "Admin");

            // Get department to set manager automatically
            int? managerId = null;
            if (createEmployeeDto.DepartmentId.HasValue && !isManager)
            {
                // For regular employees, set department's manager as their manager
                var department = await _context.Departments.FindAsync(createEmployeeDto.DepartmentId.Value);
                if (department?.ManagerId != null)
                {
                    managerId = department.ManagerId;
                }
            }

            var employee = new Employee
            {
                FirstName = createEmployeeDto.FirstName,
                LastName = createEmployeeDto.LastName,
                Email = createEmployeeDto.Email,
                EmployeeNumber = createEmployeeDto.EmployeeNumber,
                PhoneNumber = createEmployeeDto.PhoneNumber,
                HireDate = DateTime.SpecifyKind(createEmployeeDto.HireDate, DateTimeKind.Utc),
                DepartmentId = createEmployeeDto.DepartmentId,
                ManagerId = managerId, // Automatically set from department's manager for regular employees
                Username = createEmployeeDto.Username,
                PasswordHash = !string.IsNullOrEmpty(createEmployeeDto.Password) ? BCrypt.Net.BCrypt.HashPassword(createEmployeeDto.Password) : null,
                RoleId = createEmployeeDto.RoleId,
                IsActive = createEmployeeDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // If this is a manager, update the department's manager to this employee
            if (isManager && createEmployeeDto.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(createEmployeeDto.DepartmentId.Value);
                if (department != null)
                {
                    department.ManagerId = employee.Id;
                    await _context.SaveChangesAsync();
                }
            }

            return await GetEmployeeByIdAsync(employee.Id);
        }

        public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Where(e => e.IsActive)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    EmployeeNumber = e.EmployeeNumber,
                    PhoneNumber = e.PhoneNumber,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.Name : "",
                    Username = e.Username,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : "",
                    IsActive = e.IsActive,
                    CreatedDate = e.CreatedDate,
                    LastLoginDate = e.LastLoginDate
                })
                .ToListAsync();
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return null;

            return new EmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                EmployeeNumber = employee.EmployeeNumber,
                PhoneNumber = employee.PhoneNumber,
                HireDate = employee.HireDate,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department != null ? employee.Department.Name : "",
                Username = employee.Username,
                RoleId = employee.RoleId,
                RoleName = employee.Role != null ? employee.Role.Name : "",
                IsActive = employee.IsActive,
                CreatedDate = employee.CreatedDate,
                LastLoginDate = employee.LastLoginDate
            };
        }

        public async Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            // System admin's role and critical info cannot be changed
            if (employee.Id == 1)
            {
                // Only allow updating name, email and phone - not role, department or active status
                employee.FirstName = updateEmployeeDto.FirstName;
                employee.LastName = updateEmployeeDto.LastName;
                employee.Email = updateEmployeeDto.Email;
                employee.PhoneNumber = updateEmployeeDto.PhoneNumber;
                employee.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            // Check if username already exists (excluding current employee)
            if (!string.IsNullOrEmpty(updateEmployeeDto.Username))
            {
                var existingEmployee = await _context.Employees
                    .AnyAsync(e => e.Username == updateEmployeeDto.Username && e.Id != id);

                if (existingEmployee)
                    return false;
            }

            employee.FirstName = updateEmployeeDto.FirstName;
            employee.LastName = updateEmployeeDto.LastName;
            employee.Email = updateEmployeeDto.Email;
            employee.PhoneNumber = updateEmployeeDto.PhoneNumber;
            employee.DepartmentId = updateEmployeeDto.DepartmentId;
            employee.Username = updateEmployeeDto.Username;
            employee.RoleId = updateEmployeeDto.RoleId;
            employee.IsActive = updateEmployeeDto.IsActive;
            employee.UpdatedDate = DateTime.UtcNow;

            // Update password only if provided
            if (!string.IsNullOrEmpty(updateEmployeeDto.Password))
            {
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateEmployeeDto.Password);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            // System admin cannot be deactivated
            if (employee.Id == 1)
                throw new InvalidOperationException("Sistem admini deaktif edilemez");

            employee.IsActive = false;
            employee.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive)
                .ToListAsync();
        }
    }
}