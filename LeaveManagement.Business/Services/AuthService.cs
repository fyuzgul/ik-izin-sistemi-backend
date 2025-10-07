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

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("EmployeeId", user.EmployeeId.ToString()),
                new Claim("DepartmentId", user.Employee.DepartmentId?.ToString() ?? "0"),
                new Claim("DepartmentName", user.Employee.Department?.Name ?? ""),
                new Claim("EmployeeName", $"{user.Employee.FirstName} {user.Employee.LastName}"),
                new Claim("IsActive", user.IsActive.ToString())
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
            var user = await _context.Users
                .Include(u => u.Employee)
                .ThenInclude(e => e.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token
            };
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if username or email already exists
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username == createUserDto.Username || u.Email == createUserDto.Email);

            if (existingUser)
            {
                return null;
            }

            var user = new User
            {
                Username = createUserDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Email = createUserDto.Email,
                EmployeeId = createUserDto.EmployeeId,
                RoleId = createUserDto.RoleId,
                IsActive = createUserDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Employee)
                .Include(u => u.Role)
                .Include(u => u.Employee.Department)
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    EmployeeName = $"{u.Employee.FirstName} {u.Employee.LastName}",
                    RoleName = u.Role.Name,
                    DepartmentName = u.Employee.Department != null ? u.Employee.Department.Name : "",
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Employee)
                .Include(u => u.Role)
                .Include(u => u.Employee.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                EmployeeName = $"{user.Employee.FirstName} {user.Employee.LastName}",
                RoleName = user.Role.Name,
                DepartmentName = user.Employee.Department?.Name ?? "",
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };
        }

        public async Task<bool> UpdateUserAsync(int userId, CreateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Check if username or email already exists (excluding current user)
            var existingUser = await _context.Users
                .AnyAsync(u => (u.Username == updateUserDto.Username || u.Email == updateUserDto.Email) && u.Id != userId);

            if (existingUser)
                return false;

            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            user.RoleId = updateUserDto.RoleId;
            user.IsActive = updateUserDto.IsActive;
            user.UpdatedDate = DateTime.UtcNow;

            // Update password only if provided
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId && r.IsActive);
        }
    }
}
