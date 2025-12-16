namespace LeaveManagement.Business.Models
{
    public class ExcelImportPreviewDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    public class ExcelImportResultDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> SuccessMessages { get; set; } = new List<string>();
    }

    // Department Excel Import
    public class DepartmentExcelRow
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public string? ManagerEmail { get; set; } // Manager'ı email ile bulacağız
    }

    // Employee Excel Import
    public class EmployeeExcelRow
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string TitleName { get; set; } = string.Empty;
        public string? ManagerEmail { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool WorksOnSaturday { get; set; } = false;
    }
}

