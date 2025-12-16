using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Business.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly LeaveManagementDbContext _context;

        public DepartmentService(LeaveManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    ManagerId = d.ManagerId,
                    ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : "",
                    IsActive = d.IsActive,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return null;

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : "",
                IsActive = department.IsActive,
                CreatedDate = department.CreatedDate,
                UpdatedDate = department.UpdatedDate
            };
        }

        public async Task<DepartmentDto?> CreateAsync(CreateDepartmentDto createDepartmentDto)
        {
            // Check if department name already exists
            var existingDepartment = await _context.Departments
                .AnyAsync(d => d.Name == createDepartmentDto.Name);

            if (existingDepartment)
            {
                return null;
            }

            var department = new Department
            {
                Name = createDepartmentDto.Name,
                Description = createDepartmentDto.Description,
                ManagerId = null, // Yönetici daha sonra atanacak
                IsActive = createDepartmentDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(department.Id);
        }

        public async Task<bool> UpdateAsync(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            // System protected departments cannot be updated
            if (department.IsSystem)
                throw new InvalidOperationException("Sistem departmanları güncellenemez");

            // Check if department name already exists (excluding current department)
            var existingDepartment = await _context.Departments
                .AnyAsync(d => d.Name == updateDepartmentDto.Name && d.Id != id);

            if (existingDepartment)
                return false;

            var oldName = department.Name;
            department.Name = updateDepartmentDto.Name;
            department.Description = updateDepartmentDto.Description;
            department.ManagerId = updateDepartmentDto.ManagerId > 0 ? updateDepartmentDto.ManagerId : null;
            department.IsActive = updateDepartmentDto.IsActive;
            department.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (department == null)
                return false;

            // System protected departments cannot be deleted
            if (department.IsSystem)
                throw new InvalidOperationException("Sistem departmanları silinemez");

            // Clear manager reference when department is deleted
            department.ManagerId = null;
            department.IsActive = false;
            department.UpdatedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            department.IsActive = true;
            department.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            department.IsActive = false;
            department.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
