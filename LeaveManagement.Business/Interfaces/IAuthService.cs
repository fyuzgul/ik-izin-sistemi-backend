using LeaveManagement.Business.Models;
using LeaveManagement.Entity;

namespace LeaveManagement.Business.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<EmployeeDto?> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);
        Task<List<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<bool> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto);
        Task<bool> DeactivateEmployeeAsync(int id);
        Task<List<Title>> GetTitlesAsync();
    }
}