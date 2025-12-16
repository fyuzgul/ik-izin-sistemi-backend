using LeaveManagement.Business.Models;
using Microsoft.AspNetCore.Http;

namespace LeaveManagement.Business.Interfaces
{
    public interface IExcelImportService
    {
        Task<ExcelImportPreviewDto> PreviewDepartmentsFromExcelAsync(IFormFile file);
        Task<ExcelImportResultDto> ImportDepartmentsFromExcelAsync(IFormFile file);
        
        Task<ExcelImportPreviewDto> PreviewEmployeesFromExcelAsync(IFormFile file);
        Task<ExcelImportResultDto> ImportEmployeesFromExcelAsync(IFormFile file);
    }
}

