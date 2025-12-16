using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;

namespace LeaveManagement.Business.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            // Exclude system admin from employee lists
            var employees = await _unitOfWork.Employees.FindAsync(e => !e.IsSystemAdmin);
            var result = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                result.Add(await MapToDto(employee));
            }

            return result;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            return employee != null ? await MapToDto(employee) : null;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            // Exclude system admin from department employee lists
            var employees = await _unitOfWork.Employees.FindAsync(e => e.DepartmentId == departmentId && e.IsActive && !e.IsSystemAdmin);
            var result = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                result.Add(await MapToDto(employee));
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(int managerId)
        {
            // Artık ManagerId kullanmıyoruz, bu metod departman yöneticisi için kullanılacak
            // Departman yöneticisinin departmanındaki tüm çalışanları getir
            var manager = await _unitOfWork.Employees.GetByIdAsync(managerId);
            if (manager?.DepartmentId == null)
                return new List<EmployeeDto>();

            // Exclude system admin from subordinates list
            var employees = await _unitOfWork.Employees.FindAsync(e => e.DepartmentId == manager.DepartmentId && e.IsActive && e.Id != managerId && !e.IsSystemAdmin);
            var result = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                result.Add(await MapToDto(employee));
            }

            return result;
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = new Employee
            {
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                Email = employeeDto.Email,
                EmployeeNumber = employeeDto.EmployeeNumber,
                PhoneNumber = employeeDto.PhoneNumber,
                HireDate = DateTime.SpecifyKind(employeeDto.HireDate, DateTimeKind.Utc),
                DepartmentId = employeeDto.DepartmentId,
                Username = employeeDto.Username,
                PasswordHash = !string.IsNullOrEmpty(employeeDto.Password) ? BCrypt.Net.BCrypt.HashPassword(employeeDto.Password) : null,
                TitleId = employeeDto.TitleId,
                WorksOnSaturday = employeeDto.WorksOnSaturday,
                IsActive = employeeDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            // Initialize leave balances for the new employee
            await InitializeLeaveBalancesForEmployee(employee.Id);

            return await MapToDto(employee);
        }

        public async Task<bool> UpdateEmployeeAsync(int id, EmployeeDto employeeDto)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.FirstName = employeeDto.FirstName;
            employee.LastName = employeeDto.LastName;
            employee.Email = employeeDto.Email;
            employee.EmployeeNumber = employeeDto.EmployeeNumber;
            employee.PhoneNumber = employeeDto.PhoneNumber;
            employee.HireDate = DateTime.SpecifyKind(employeeDto.HireDate, DateTimeKind.Utc);
            employee.DepartmentId = employeeDto.DepartmentId;
            employee.Username = employeeDto.Username;
            employee.TitleId = employeeDto.TitleId;
            employee.WorksOnSaturday = employeeDto.WorksOnSaturday;
            employee.IsActive = employeeDto.IsActive;
            employee.UpdatedDate = DateTime.UtcNow;

            // Update password only if provided
            if (!string.IsNullOrEmpty(employeeDto.Password))
            {
                employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeeDto.Password);
            }

            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return false;

            await _unitOfWork.Employees.DeleteAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateEmployeeAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.IsActive = false;
            employee.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateEmployeeAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
                return false;

            employee.IsActive = true;
            employee.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task<EmployeeDto> MapToDto(Employee employee)
        {
            var department = employee.DepartmentId.HasValue 
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId.Value) 
                : null;

            var title = employee.TitleId.HasValue 
                ? await _unitOfWork.Titles.GetByIdAsync(employee.TitleId.Value) 
                : null;

            var manager = employee.ManagerId.HasValue
                ? await _unitOfWork.Employees.GetByIdAsync(employee.ManagerId.Value)
                : null;

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
                DepartmentName = department?.Name ?? string.Empty,
                Username = employee.Username,
                TitleId = employee.TitleId,
                TitleName = title?.Name ?? string.Empty,
                ManagerId = employee.ManagerId,
                ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
                WorksOnSaturday = employee.WorksOnSaturday,
                IsActive = employee.IsActive,
                CreatedDate = employee.CreatedDate,
                LastLoginDate = employee.LastLoginDate
            };
        }

        private async Task InitializeLeaveBalancesForEmployee(int employeeId)
        {
            var currentYear = DateTime.Now.Year;
            var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();

            // Only create balances for leave types that require balance tracking (e.g., Annual Leave)
            foreach (var leaveType in leaveTypes.Where(lt => lt.IsActive && lt.RequiresBalance))
            {
                var leaveBalance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveType.Id,
                    Year = currentYear,
                    TotalDays = leaveType.MaxDaysPerYear,
                    UsedDays = 0,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.LeaveBalances.AddAsync(leaveBalance);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}


