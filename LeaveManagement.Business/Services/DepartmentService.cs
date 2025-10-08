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

            // Create department-specific roles
            await CreateDepartmentRolesAsync(department.Id, department.Name);

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

            // Update department-specific roles if name changed
            if (oldName != department.Name)
            {
                await UpdateDepartmentRolesAsync(department.Id, oldName, department.Name);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            // System protected departments cannot be deleted
            if (department.IsSystem)
                throw new InvalidOperationException("Sistem departmanları silinemez");

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

        private async Task CreateDepartmentRolesAsync(int departmentId, string departmentName)
        {
            try
            {
                // Create Department Manager role
                var departmentManagerRole = new Role
                {
                    Name = $"{departmentName}Manager",
                    Description = $"{departmentName} Departman Yöneticisi",
                    CanManageEmployees = false,
                    CanManageDepartments = false,
                    CanManageLeaveTypes = false,
                    CanApproveLeaveRequests = true,
                    CanViewAllLeaveRequests = false,
                    CanManageSystemSettings = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Create Department Employee role
                var departmentEmployeeRole = new Role
                {
                    Name = $"{departmentName}Employee",
                    Description = $"{departmentName} Çalışanı",
                    CanManageEmployees = false,
                    CanManageDepartments = false,
                    CanManageLeaveTypes = false,
                    CanApproveLeaveRequests = false,
                    CanViewAllLeaveRequests = false,
                    CanManageSystemSettings = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Roles.AddRange(departmentManagerRole, departmentEmployeeRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the department creation
                Console.WriteLine($"Error creating department roles: {ex.Message}");
            }
        }

        private async Task UpdateDepartmentRolesAsync(int departmentId, string oldName, string newName)
        {
            // Update Department Manager role
            var departmentManagerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == $"{oldName}Manager");

            if (departmentManagerRole != null)
            {
                departmentManagerRole.Name = $"{newName}Manager";
                departmentManagerRole.Description = $"{newName} Departman Yöneticisi";
                departmentManagerRole.UpdatedDate = DateTime.UtcNow;
            }

            // Update Department Employee role
            var departmentEmployeeRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == $"{oldName}Employee");

            if (departmentEmployeeRole != null)
            {
                departmentEmployeeRole.Name = $"{newName}Employee";
                departmentEmployeeRole.Description = $"{newName} Çalışanı";
                departmentEmployeeRole.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
