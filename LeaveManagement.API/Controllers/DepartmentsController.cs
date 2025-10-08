using Microsoft.AspNetCore.Mvc;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var departments = await _departmentService.GetAllAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var department = await _departmentService.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound(new { message = "Departman bulunamadı" });
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            try
            {
                var result = await _departmentService.CreateAsync(createDepartmentDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Departman oluşturulamadı. Departman adı zaten kullanımda." });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
        {
            try
            {
                var result = await _departmentService.UpdateAsync(id, updateDepartmentDto);
                if (!result)
                {
                    return BadRequest(new { message = "Departman güncellenemedi" });
                }

                return Ok(new { message = "Departman başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _departmentService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Departman bulunamadı" });
                }

                return Ok(new { message = "Departman başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var result = await _departmentService.ActivateAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Departman bulunamadı" });
                }

                return Ok(new { message = "Departman başarıyla aktifleştirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _departmentService.DeactivateAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Departman bulunamadı" });
                }

                return Ok(new { message = "Departman başarıyla deaktive edildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }
}