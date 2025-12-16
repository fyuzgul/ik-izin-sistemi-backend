using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/holidays")]
    [Authorize]
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidaysController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var holidays = await _holidayService.GetAllHolidaysAsync();
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            try
            {
                var holidays = await _holidayService.GetHolidaysByYearAsync(year);
                return Ok(holidays);
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
                var holiday = await _holidayService.GetHolidayByIdAsync(id);
                if (holiday == null)
                    return NotFound(new { message = "Bayram tatili bulunamadı" });

                return Ok(holiday);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHolidayDto createDto)
        {
            try
            {
                var result = await _holidayService.CreateHolidayAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHolidayDto updateDto)
        {
            try
            {
                var result = await _holidayService.UpdateHolidayAsync(id, updateDto);
                if (!result)
                    return NotFound(new { message = "Bayram tatili bulunamadı" });

                return Ok(new { message = "Bayram tatili başarıyla güncellendi" });
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
                var result = await _holidayService.DeleteHolidayAsync(id);
                if (!result)
                    return NotFound(new { message = "Bayram tatili bulunamadı" });

                return Ok(new { message = "Bayram tatili başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("generate/{year}")]
        public async Task<IActionResult> GenerateHolidaysForYear(int year)
        {
            try
            {
                var result = await _holidayService.EnsureHolidaysForYearAsync(year);
                if (!result)
                    return BadRequest(new { message = $"Bu yıl ({year}) için resmi tatiller zaten oluşturulmuş" });

                var holidays = await _holidayService.GetHolidaysByYearAsync(year);
                return Ok(new { message = $"{year} yılı için resmi tatiller başarıyla oluşturuldu", holidays });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpGet("generate/{year}")]
        public async Task<IActionResult> PreviewHolidaysForYear(int year)
        {
            try
            {
                var holidays = await _holidayService.GenerateOfficialHolidaysForYearAsync(year);
                return Ok(holidays);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }
}

