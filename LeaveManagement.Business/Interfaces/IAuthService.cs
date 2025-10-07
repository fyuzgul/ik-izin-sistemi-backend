using LeaveManagement.Business.Models;
using LeaveManagement.Entity;

namespace LeaveManagement.Business.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(int userId, CreateUserDto updateUserDto);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);
    }
}
