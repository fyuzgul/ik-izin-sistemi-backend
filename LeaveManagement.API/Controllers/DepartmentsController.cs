using Microsoft.AspNetCore.Mvc;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAllDepartments()
        {
            try
            {
                var departments = await _unitOfWork.Departments.GetAllAsync();
                var departmentDtos = new List<DepartmentDto>();

                foreach (var department in departments)
                {
                    var manager = department.ManagerId.HasValue 
                        ? await _unitOfWork.Employees.GetByIdAsync(department.ManagerId.Value) 
                        : null;

                    departmentDtos.Add(new DepartmentDto
                    {
                        Id = department.Id,
                        Name = department.Name,
                        Description = department.Description,
                        ManagerId = department.ManagerId,
                        ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
                        IsActive = department.IsActive,
                        CreatedDate = department.CreatedDate,
                        UpdatedDate = department.UpdatedDate
                    });
                }

                return Ok(departmentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                    return NotFound();

                var manager = department.ManagerId.HasValue 
                    ? await _unitOfWork.Employees.GetByIdAsync(department.ManagerId.Value) 
                    : null;

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    ManagerId = department.ManagerId,
                    ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
                    IsActive = department.IsActive,
                    CreatedDate = department.CreatedDate,
                    UpdatedDate = department.UpdatedDate
                };

                return Ok(departmentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var department = new Department
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    ManagerId = createDto.ManagerId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Departments.AddAsync(department);
                await _unitOfWork.SaveChangesAsync();

                var manager = department.ManagerId.HasValue 
                    ? await _unitOfWork.Employees.GetByIdAsync(department.ManagerId.Value) 
                    : null;

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    ManagerId = department.ManagerId,
                    ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : null,
                    IsActive = department.IsActive,
                    CreatedDate = department.CreatedDate,
                    UpdatedDate = department.UpdatedDate
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, departmentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                    return NotFound();

                department.Name = updateDto.Name;
                department.Description = updateDto.Description;
                department.ManagerId = updateDto.ManagerId;
                department.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Departments.UpdateAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Department updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                    return NotFound();

                // Check if there are employees in this department
                var hasEmployees = await _unitOfWork.Employees.ExistsAsync(e => e.DepartmentId == id);
                if (hasEmployees)
                    return BadRequest("Cannot delete department that has employees");

                await _unitOfWork.Departments.DeleteAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Department deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                    return NotFound();

                department.IsActive = true;
                department.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Departments.UpdateAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Department activated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateDepartment(int id)
        {
            try
            {
                var department = await _unitOfWork.Departments.GetByIdAsync(id);
                if (department == null)
                    return NotFound();

                department.IsActive = false;
                department.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.Departments.UpdateAsync(department);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Department deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ManagerId { get; set; }
    }
}


