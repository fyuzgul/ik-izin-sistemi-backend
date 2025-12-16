using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LeaveManagement.Business.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LeaveManagementDbContext _context;

        private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

        public ExcelImportService(IUnitOfWork unitOfWork, LeaveManagementDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private static string NormalizeForComparison(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            
            // Türkçe locale ile küçük harfe çevir ve trim yap
            return str.Trim().ToLower(TurkishCulture);
        }

        private static bool EqualsIgnoreCase(string str1, string str2)
        {
            if (str1 == null && str2 == null) return true;
            if (str1 == null || str2 == null) return false;
            
            var normalized1 = NormalizeForComparison(str1);
            var normalized2 = NormalizeForComparison(str2);
            
            return normalized1 == normalized2;
        }

        public async Task<ExcelImportPreviewDto> PreviewDepartmentsFromExcelAsync(IFormFile file)
        {
            var result = new ExcelImportPreviewDto { IsValid = true };

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet.Dimension == null)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Excel dosyası boş veya geçersiz format";
                    return result;
                }

                // Read headers
                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? "";
                    headers.Add(header);
                }
                result.Headers = headers;

                // Validate headers
                var requiredHeaders = new[] { "Name", "Departman Adı", "Departman" };
                var hasValidHeader = headers.Any(h => requiredHeaders.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                
                if (!hasValidHeader)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Excel dosyasında 'Name', 'Departman Adı' veya 'Departman' sütunu bulunamadı";
                    return result;
                }

                // Read rows
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var rowData = new Dictionary<string, object>();
                    bool hasData = false;

                    for (int col = 1; col <= headers.Count; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(cellValue))
                            hasData = true;
                        rowData[headers[col - 1]] = cellValue ?? "";
                    }

                    if (hasData)
                    {
                        result.Rows.Add(rowData);
                    }
                }

                // Validate rows
                var nameIndex = headers.FindIndex(h => new[] { "Name", "Departman Adı", "Departman" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    var row = result.Rows[i];
                    var name = row.Values.ElementAt(nameIndex)?.ToString()?.Trim();
                    
                    if (string.IsNullOrEmpty(name))
                    {
                        result.ValidationErrors.Add($"Satır {i + 2}: Departman adı boş olamaz");
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Excel dosyası okunurken hata: {ex.Message}";
            }

            return result;
        }

        public async Task<ExcelImportResultDto> ImportDepartmentsFromExcelAsync(IFormFile file)
        {
            var result = new ExcelImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? "");
                }

                var nameIndex = headers.FindIndex(h => new[] { "Name", "Departman Adı", "Departman" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var descIndex = headers.FindIndex(h => new[] { "Description", "Açıklama" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var codeIndex = headers.FindIndex(h => new[] { "Code", "Kod" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));

                // Get all existing departments for case-insensitive comparison
                var existingDepartments = await _context.Departments.ToListAsync();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var name = worksheet.Cells[row, nameIndex + 1].Value?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(name))
                            continue;

                        // Check if department already exists (case-insensitive with Turkish locale)
                        var existing = existingDepartments.FirstOrDefault(d => EqualsIgnoreCase(d.Name, name));

                        if (existing != null)
                        {
                            result.ErrorCount++;
                            result.Errors.Add($"Satır {row}: '{name}' departmanı zaten mevcut - eklenmedi");
                            continue;
                        }

                        var department = new Department
                        {
                            Name = name,
                            Description = descIndex >= 0 ? worksheet.Cells[row, descIndex + 1].Value?.ToString()?.Trim() : null,
                            Code = codeIndex >= 0 ? worksheet.Cells[row, codeIndex + 1].Value?.ToString()?.Trim() : null,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _unitOfWork.Departments.AddAsync(department);
                        result.SuccessCount++;
                        result.SuccessMessages.Add($"'{name}' departmanı başarıyla oluşturuldu");
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Satır {row}: {ex.Message}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Genel hata: {ex.Message}");
            }

            return result;
        }

        public async Task<ExcelImportPreviewDto> PreviewEmployeesFromExcelAsync(IFormFile file)
        {
            var result = new ExcelImportPreviewDto { IsValid = true };

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet.Dimension == null)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Excel dosyası boş veya geçersiz format";
                    return result;
                }

                // Read headers
                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? "";
                    headers.Add(header);
                }
                result.Headers = headers;

                // Validate required headers
                var requiredHeaders = new Dictionary<string, string[]>
                {
                    { "FirstName", new[] { "FirstName", "Ad", "İsim" } },
                    { "LastName", new[] { "LastName", "Soyad", "Soyisim" } },
                    { "Email", new[] { "Email", "E-posta", "Eposta" } },
                    { "EmployeeNumber", new[] { "EmployeeNumber", "Çalışan No", "Personel No", "Sicil No" } },
                    { "DepartmentName", new[] { "DepartmentName", "Departman", "Departman Adı" } },
                    { "TitleName", new[] { "TitleName", "Pozisyon", "Ünvan", "Title" } }
                };

                var missingHeaders = new List<string>();
                foreach (var req in requiredHeaders)
                {
                    if (!headers.Any(h => req.Value.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase))))
                    {
                        missingHeaders.Add($"{req.Key} ({string.Join(", ", req.Value)})");
                    }
                }

                if (missingHeaders.Any())
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Eksik sütunlar: {string.Join(", ", missingHeaders)}";
                    return result;
                }

                // Read rows
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var rowData = new Dictionary<string, object>();
                    bool hasData = false;

                    for (int col = 1; col <= headers.Count; col++)
                    {
                        var cellValue = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(cellValue))
                            hasData = true;
                        rowData[headers[col - 1]] = cellValue ?? "";
                    }

                    if (hasData)
                    {
                        result.Rows.Add(rowData);
                    }
                }

                // Validate rows
                var firstNameIndex = headers.FindIndex(h => requiredHeaders["FirstName"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var lastNameIndex = headers.FindIndex(h => requiredHeaders["LastName"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var emailIndex = headers.FindIndex(h => requiredHeaders["Email"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var empNoIndex = headers.FindIndex(h => requiredHeaders["EmployeeNumber"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var deptIndex = headers.FindIndex(h => requiredHeaders["DepartmentName"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var titleIndex = headers.FindIndex(h => requiredHeaders["TitleName"].Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    var row = result.Rows[i];
                    var firstName = row.Values.ElementAt(firstNameIndex)?.ToString()?.Trim();
                    var lastName = row.Values.ElementAt(lastNameIndex)?.ToString()?.Trim();
                    var email = row.Values.ElementAt(emailIndex)?.ToString()?.Trim();
                    var empNo = row.Values.ElementAt(empNoIndex)?.ToString()?.Trim();
                    var deptName = row.Values.ElementAt(deptIndex)?.ToString()?.Trim();
                    var titleName = row.Values.ElementAt(titleIndex)?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(firstName))
                        result.ValidationErrors.Add($"Satır {i + 2}: Ad boş olamaz");
                    if (string.IsNullOrEmpty(lastName))
                        result.ValidationErrors.Add($"Satır {i + 2}: Soyad boş olamaz");
                    if (string.IsNullOrEmpty(email))
                        result.ValidationErrors.Add($"Satır {i + 2}: E-posta boş olamaz");
                    else if (!IsValidEmail(email))
                        result.ValidationErrors.Add($"Satır {i + 2}: Geçersiz e-posta formatı: {email}");
                    if (string.IsNullOrEmpty(empNo))
                        result.ValidationErrors.Add($"Satır {i + 2}: Çalışan numarası boş olamaz");
                    if (string.IsNullOrEmpty(deptName))
                        result.ValidationErrors.Add($"Satır {i + 2}: Departman adı boş olamaz");
                    if (string.IsNullOrEmpty(titleName))
                        result.ValidationErrors.Add($"Satır {i + 2}: Pozisyon boş olamaz");
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Excel dosyası okunurken hata: {ex.Message}";
            }

            return result;
        }

        public async Task<ExcelImportResultDto> ImportEmployeesFromExcelAsync(IFormFile file)
        {
            var result = new ExcelImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? "");
                }

                // Find column indices
                var firstNameIndex = headers.FindIndex(h => new[] { "FirstName", "Ad", "İsim" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var lastNameIndex = headers.FindIndex(h => new[] { "LastName", "Soyad", "Soyisim" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var emailIndex = headers.FindIndex(h => new[] { "Email", "E-posta", "Eposta" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var empNoIndex = headers.FindIndex(h => new[] { "EmployeeNumber", "Çalışan No", "Personel No", "Sicil No" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var phoneIndex = headers.FindIndex(h => new[] { "PhoneNumber", "Telefon", "Tel" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var hireDateIndex = headers.FindIndex(h => new[] { "HireDate", "İşe Giriş", "İşe Başlama" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var deptIndex = headers.FindIndex(h => new[] { "DepartmentName", "Departman", "Departman Adı" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var titleIndex = headers.FindIndex(h => new[] { "TitleName", "Pozisyon", "Ünvan", "Title" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var managerEmailIndex = headers.FindIndex(h => new[] { "ManagerEmail", "Yönetici Email", "Yönetici E-posta" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var usernameIndex = headers.FindIndex(h => new[] { "Username", "Kullanıcı Adı" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var passwordIndex = headers.FindIndex(h => new[] { "Password", "Şifre" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));
                var worksOnSaturdayIndex = headers.FindIndex(h => new[] { "WorksOnSaturday", "Cumartesi Çalışıyor", "Cumartesi" }.Any(rh => h.Equals(rh, StringComparison.OrdinalIgnoreCase)));

                // Get all departments and titles for lookup
                var departments = await _context.Departments.ToListAsync();
                var titles = await _context.Titles.ToListAsync();
                var existingEmployees = await _context.Employees.ToListAsync();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var firstName = worksheet.Cells[row, firstNameIndex + 1].Value?.ToString()?.Trim();
                        var lastName = worksheet.Cells[row, lastNameIndex + 1].Value?.ToString()?.Trim();
                        var email = worksheet.Cells[row, emailIndex + 1].Value?.ToString()?.Trim();
                        var empNo = worksheet.Cells[row, empNoIndex + 1].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(empNo))
                            continue;

                        // Check if employee already exists (case-insensitive with Turkish locale)
                        var existingEmployee = existingEmployees.FirstOrDefault(e => 
                            EqualsIgnoreCase(e.Email, email) || EqualsIgnoreCase(e.EmployeeNumber, empNo));
                        if (existingEmployee != null)
                        {
                            result.ErrorCount++;
                            var duplicateField = existingEmployee.Email.ToLower() == email.ToLower() ? "E-posta" : "Çalışan No";
                            result.Errors.Add($"Satır {row}: '{firstName} {lastName}' - {duplicateField} zaten mevcut, eklenmedi");
                            continue;
                        }

                        // Find department (case-insensitive with Turkish locale)
                        var deptName = worksheet.Cells[row, deptIndex + 1].Value?.ToString()?.Trim();
                        var department = departments.FirstOrDefault(d => EqualsIgnoreCase(d.Name, deptName));
                        if (department == null)
                        {
                            result.ErrorCount++;
                            result.Errors.Add($"Satır {row}: '{deptName}' departmanı bulunamadı");
                            continue;
                        }

                        // Find title (case-insensitive with Turkish locale)
                        var titleName = worksheet.Cells[row, titleIndex + 1].Value?.ToString()?.Trim();
                        var title = titles.FirstOrDefault(t => EqualsIgnoreCase(t.Name, titleName));
                        if (title == null)
                        {
                            result.ErrorCount++;
                            result.Errors.Add($"Satır {row}: '{titleName}' pozisyonu bulunamadı");
                            continue;
                        }

                        // Parse hire date
                        DateTime hireDate = DateTime.UtcNow;
                        if (hireDateIndex >= 0)
                        {
                            var hireDateValue = worksheet.Cells[row, hireDateIndex + 1].Value;
                            if (hireDateValue is DateTime dt)
                                hireDate = dt;
                            else if (hireDateValue != null && DateTime.TryParse(hireDateValue.ToString(), out var parsedDate))
                                hireDate = parsedDate;
                        }

                        // Find manager if specified
                        int? managerId = null;
                        if (managerEmailIndex >= 0)
                        {
                            var managerEmail = worksheet.Cells[row, managerEmailIndex + 1].Value?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(managerEmail))
                            {
                                var manager = existingEmployees.FirstOrDefault(e => EqualsIgnoreCase(e.Email, managerEmail));
                                if (manager != null)
                                    managerId = manager.Id;
                            }
                        }

                        // If no manager specified, use department manager
                        if (!managerId.HasValue && department.ManagerId.HasValue)
                        {
                            managerId = department.ManagerId;
                        }

                        var employee = new Employee
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            EmployeeNumber = empNo,
                            PhoneNumber = phoneIndex >= 0 ? worksheet.Cells[row, phoneIndex + 1].Value?.ToString()?.Trim() : null,
                            HireDate = DateTime.SpecifyKind(hireDate, DateTimeKind.Utc),
                            DepartmentId = department.Id,
                            TitleId = title.Id,
                            ManagerId = managerId,
                            Username = usernameIndex >= 0 ? worksheet.Cells[row, usernameIndex + 1].Value?.ToString()?.Trim() : null,
                            PasswordHash = passwordIndex >= 0 && !string.IsNullOrEmpty(worksheet.Cells[row, passwordIndex + 1].Value?.ToString()) 
                                ? BCrypt.Net.BCrypt.HashPassword(worksheet.Cells[row, passwordIndex + 1].Value.ToString()) 
                                : null,
                            WorksOnSaturday = worksOnSaturdayIndex >= 0 && 
                                (worksheet.Cells[row, worksOnSaturdayIndex + 1].Value?.ToString()?.ToLower() == "evet" || 
                                 worksheet.Cells[row, worksOnSaturdayIndex + 1].Value?.ToString()?.ToLower() == "yes" ||
                                 worksheet.Cells[row, worksOnSaturdayIndex + 1].Value?.ToString() == "1"),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _unitOfWork.Employees.AddAsync(employee);
                        result.SuccessCount++;
                        result.SuccessMessages.Add($"'{firstName} {lastName}' başarıyla oluşturuldu");
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Satır {row}: {ex.Message}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Genel hata: {ex.Message}");
            }

            return result;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}

