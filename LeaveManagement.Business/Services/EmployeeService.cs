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
            var employees = await _unitOfWork.Employees.GetAllAsync();
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
            var employees = await _unitOfWork.Employees.FindAsync(e => e.DepartmentId == departmentId && e.IsActive);
            var result = new List<EmployeeDto>();

            foreach (var employee in employees)
            {
                result.Add(await MapToDto(employee));
            }

            return result;
        }

        public async Task<IEnumerable<EmployeeDto>> GetSubordinatesAsync(int managerId)
        {
            var employees = await _unitOfWork.Employees.FindAsync(e => e.ManagerId == managerId && e.IsActive);
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
                DepartmentId = employeeDto.DepartmentId,
                ManagerId = employeeDto.ManagerId,
                IsActive = true,
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
            employee.DepartmentId = employeeDto.DepartmentId;
            employee.ManagerId = employeeDto.ManagerId;
            employee.IsActive = employeeDto.IsActive;
            employee.UpdatedDate = DateTime.UtcNow;

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
                DepartmentId = employee.DepartmentId,
                DepartmentName = department?.Name,
                ManagerId = employee.ManagerId,
                ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
                IsActive = employee.IsActive
            };
        }

        private async Task InitializeLeaveBalancesForEmployee(int employeeId)
        {
            var currentYear = DateTime.Now.Year;
            var leaveTypes = await _unitOfWork.LeaveTypes.GetAllAsync();

            foreach (var leaveType in leaveTypes.Where(lt => lt.IsActive))
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


