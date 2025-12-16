using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeaveManagement.Business.Interfaces;
using LeaveManagement.API.Extensions;

namespace LeaveManagement.API.Controllers
{
    [ApiController]
    [Route("api/excel-import")]
    [Authorize]
    public class ExcelImportController : ControllerBase
    {
        private readonly IExcelImportService _excelImportService;

        public ExcelImportController(IExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpPost("departments/preview")]
        public async Task<IActionResult> PreviewDepartments([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Dosya seçilmedi" });

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir" });

                var result = await _excelImportService.PreviewDepartmentsFromExcelAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("departments/import")]
        public async Task<IActionResult> ImportDepartments([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Dosya seçilmedi" });

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir" });

                var result = await _excelImportService.ImportDepartmentsFromExcelAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("employees/preview")]
        public async Task<IActionResult> PreviewEmployees([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Dosya seçilmedi" });

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir" });

                var result = await _excelImportService.PreviewEmployeesFromExcelAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }

        [HttpPost("employees/import")]
        public async Task<IActionResult> ImportEmployees([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Dosya seçilmedi" });

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                    !file.FileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Sadece Excel dosyaları (.xlsx, .xls) yüklenebilir" });

                var result = await _excelImportService.ImportEmployeesFromExcelAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası", error = ex.Message });
            }
        }
    }
}

