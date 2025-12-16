using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.API.Extensions;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/goal-cards")]
    [Authorize]
    public class GoalCardsController : ControllerBase
    {
        private readonly IGoalCardService _goalCardService;

        public GoalCardsController(IGoalCardService goalCardService)
        {
            _goalCardService = goalCardService;
        }

        // Goal Types
        [HttpGet("goal-types")]
        public async Task<IActionResult> GetAllGoalTypes()
        {
            try
            {
                var goalTypes = await _goalCardService.GetAllGoalTypesAsync();
                return Ok(goalTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("goal-types/{id}")]
        public async Task<IActionResult> GetGoalTypeById(int id)
        {
            try
            {
                var goalType = await _goalCardService.GetGoalTypeByIdAsync(id);
                if (goalType == null)
                    return NotFound(new { message = "Hedef türü bulunamadı" });

                return Ok(goalType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("goal-types")]
        public async Task<IActionResult> CreateGoalType([FromBody] CreateGoalTypeDto createDto)
        {
            try
            {
                var result = await _goalCardService.CreateGoalTypeAsync(createDto);
                return CreatedAtAction(nameof(GetGoalTypeById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("goal-types/{id}")]
        public async Task<IActionResult> UpdateGoalType(int id, [FromBody] UpdateGoalTypeDto updateDto)
        {
            try
            {
                var result = await _goalCardService.UpdateGoalTypeAsync(id, updateDto);
                if (!result)
                    return NotFound(new { message = "Hedef türü bulunamadı" });

                return Ok(new { message = "Hedef türü başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("goal-types/{id}")]
        public async Task<IActionResult> DeleteGoalType(int id)
        {
            try
            {
                var result = await _goalCardService.DeleteGoalTypeAsync(id);
                if (!result)
                    return NotFound(new { message = "Hedef türü bulunamadı" });

                return Ok(new { message = "Hedef türü başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        // Goal Card Templates
        [HttpGet("templates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            try
            {
                var templates = await _goalCardService.GetAllGoalCardTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            try
            {
                var template = await _goalCardService.GetGoalCardTemplateByIdAsync(id);
                if (template == null)
                    return NotFound(new { message = "Hedef kartı şablonu bulunamadı" });

                return Ok(template);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("templates/department/{departmentId}/title/{titleId}")]
        public async Task<IActionResult> GetTemplatesByDepartmentAndTitle(int departmentId, int titleId)
        {
            try
            {
                var templates = await _goalCardService.GetGoalCardTemplatesByDepartmentAndTitleAsync(departmentId, titleId);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateGoalCardTemplateDto createDto)
        {
            try
            {
                var employeeId = User.GetEmployeeId();
                if (employeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });

                var result = await _goalCardService.CreateGoalCardTemplateAsync(createDto, employeeId.Value);
                return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateGoalCardTemplateDto updateDto)
        {
            try
            {
                var result = await _goalCardService.UpdateGoalCardTemplateAsync(id, updateDto);
                if (!result)
                    return NotFound(new { message = "Hedef kartı şablonu bulunamadı" });

                return Ok(new { message = "Hedef kartı şablonu başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var result = await _goalCardService.DeleteGoalCardTemplateAsync(id);
                if (!result)
                    return NotFound(new { message = "Hedef kartı şablonu bulunamadı" });

                return Ok(new { message = "Hedef kartı şablonu başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        // Employee Goal Cards
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetEmployeeGoalCards(int employeeId)
        {
            try
            {
                var goalCards = await _goalCardService.GetEmployeeGoalCardsByEmployeeIdAsync(employeeId);
                return Ok(goalCards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("manager/{managerId}")]
        public async Task<IActionResult> GetGoalCardsByManager(int managerId)
        {
            try
            {
                var goalCards = await _goalCardService.GetEmployeeGoalCardsByManagerIdAsync(managerId);
                return Ok(goalCards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("employee-goal-card/{id}")]
        public async Task<IActionResult> GetEmployeeGoalCardById(int id)
        {
            try
            {
                var goalCard = await _goalCardService.GetEmployeeGoalCardByIdAsync(id);
                if (goalCard == null)
                    return NotFound(new { message = "Hedef kartı bulunamadı" });

                return Ok(goalCard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("employee-goal-card")]
        public async Task<IActionResult> CreateEmployeeGoalCard([FromBody] CreateEmployeeGoalCardDto createDto)
        {
            try
            {
                var employeeId = User.GetEmployeeId();
                if (employeeId == null)
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });

                var result = await _goalCardService.CreateEmployeeGoalCardAsync(createDto, employeeId.Value);
                return CreatedAtAction(nameof(GetEmployeeGoalCardById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("employee-goal-card/{id}")]
        public async Task<IActionResult> UpdateEmployeeGoalCard(int id, [FromBody] UpdateEmployeeGoalCardDto updateDto)
        {
            try
            {
                var result = await _goalCardService.UpdateEmployeeGoalCardAsync(id, updateDto);
                if (!result)
                    return NotFound(new { message = "Hedef kartı bulunamadı" });

                return Ok(new { message = "Hedef kartı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpDelete("employee-goal-card/{id}")]
        public async Task<IActionResult> DeleteEmployeeGoalCard(int id)
        {
            try
            {
                var result = await _goalCardService.DeleteEmployeeGoalCardAsync(id);
                if (!result)
                    return NotFound(new { message = "Hedef kartı bulunamadı" });

                return Ok(new { message = "Hedef kartı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }
}

