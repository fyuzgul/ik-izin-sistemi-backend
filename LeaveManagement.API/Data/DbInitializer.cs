using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace LeaveManagement.API.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LeaveManagementDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if we already have data
            if (await context.Roles.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Create System Roles
            var roles = new[]
            {
                new Role { Id = 1, Name = "Admin", Description = "Sistem Yöneticisi - Tüm yetkilere sahip", IsSystem = true, CreatedDate = DateTime.UtcNow },
                new Role { Id = 2, Name = "İK Müdürü", Description = "İnsan Kaynakları Müdürü", IsSystem = false, CreatedDate = DateTime.UtcNow },
                new Role { Id = 3, Name = "Yönetici", Description = "Departman Yöneticisi", IsSystem = false, CreatedDate = DateTime.UtcNow },
                new Role { Id = 4, Name = "Çalışan", Description = "Normal Çalışan", IsSystem = false, CreatedDate = DateTime.UtcNow }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            // Create HR Department (System Protected)
            var hrDepartment = new Department
            {
                Name = "İnsan Kaynakları",
                Description = "İnsan Kaynakları Departmanı",
                Code = "HR",
                IsActive = true,
                IsSystem = true, // System protected - cannot be deleted
                CreatedDate = DateTime.UtcNow
            };
            await context.Departments.AddAsync(hrDepartment);
            await context.SaveChangesAsync();

            // Create System Admin Employee (System Protected)
            var adminEmployee = new Employee
            {
                FirstName = "Sistem",
                LastName = "Admin",
                Email = "admin@company.com",
                EmployeeNumber = "SYS001",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                RoleId = 1, // Admin
                DepartmentId = hrDepartment.Id, // Use generated ID
                IsActive = true,
                HireDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();

            // Update HR Department manager to admin
            hrDepartment.ManagerId = adminEmployee.Id;
            context.Departments.Update(hrDepartment);
            await context.SaveChangesAsync();

            // Create Default Leave Types
            var leaveTypes = new[]
            {
                new LeaveType 
                { 
                    Name = "Yıllık İzin", 
                    Description = "Yıllık ücretli izin hakkı",
                    MaxDaysPerYear = 14,
                    RequiresApproval = true,
                    IsPaid = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Mazeret İzni", 
                    Description = "Evlilik, ölüm vb. durumlar için mazeret izni",
                    MaxDaysPerYear = 5,
                    RequiresApproval = true,
                    IsPaid = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Hastalık İzni", 
                    Description = "Sağlık sorunları için hastalık izni",
                    MaxDaysPerYear = 10,
                    RequiresApproval = true,
                    IsPaid = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Ücretsiz İzin", 
                    Description = "Ücret kesilmesi ile kullanılan izin",
                    MaxDaysPerYear = 30,
                    RequiresApproval = true,
                    IsPaid = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };
            await context.LeaveTypes.AddRangeAsync(leaveTypes);
            await context.SaveChangesAsync();
        }
    }
}
