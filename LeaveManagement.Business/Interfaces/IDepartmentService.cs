using LeaveManagement.Business.Models;

namespace LeaveManagement.Business.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<DepartmentDto?> CreateAsync(CreateDepartmentDto createDepartmentDto);
        Task<bool> UpdateAsync(int id, UpdateDepartmentDto updateDepartmentDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}


