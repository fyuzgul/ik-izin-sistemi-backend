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

            // Create Titles if they don't exist
            if (!await context.Titles.AnyAsync())
            {
                var titles = new[]
                {
                    new Title { Id = 1, Name = "Yönetici", Description = "Yönetici", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 2, Name = "Uzman", Description = "Uzman", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 3, Name = "Uzman Yardımcısı", Description = "Uzman Yardımcısı", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 4, Name = "Takım Lideri Operatör", Description = "Takım Lideri Operatör", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 5, Name = "Güvenlik Personeli", Description = "Güvenlik Personeli", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 6, Name = "Şoför", Description = "Şoför", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 7, Name = "Direktör", Description = "Direktör", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 8, Name = "Ambalaj Şefi", Description = "Ambalaj Şefi", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 9, Name = "PLASTİKHANE ŞEFİ", Description = "PLASTİKHANE ŞEFİ", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 10, Name = "VARDİYA ŞEFİ", Description = "VARDİYA ŞEFİ", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 11, Name = "BAHÇIVAN", Description = "BAHÇIVAN", IsActive = true, CreatedDate = DateTime.UtcNow },
                    new Title { Id = 12, Name = "Personel", Description = "Personel", IsActive = true, CreatedDate = DateTime.UtcNow }
                };
                await context.Titles.AddRangeAsync(titles);
                await context.SaveChangesAsync();
            }

            // Always check and create/update admin
            var admin = await context.Employees.FirstOrDefaultAsync(e => e.Username == "admin");
            if (admin == null)
            {
                await CreateAdminUserAsync(context);
            }
            else if (!admin.IsSystemAdmin)
            {
                // Update existing admin to be system admin and remove from department
                admin.IsSystemAdmin = true;
                admin.DepartmentId = null;
                context.Employees.Update(admin);
                await context.SaveChangesAsync();
            }

            // Check if we already have all data seeded
            if (await context.LeaveTypes.AnyAsync())
            {
                return; // DB has been seeded
            }

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
                    RequiresBalance = true, // Bakiye kontrolü gerekli
                    DeductsFromBalance = true, // Bakiyeden düşer
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Mazeret İzni", 
                    Description = "Doğum, ölüm, evlilik vb. durumlar için mazeret izni",
                    MaxDaysPerYear = 0, // Sınırsız
                    RequiresApproval = true,
                    IsPaid = true,
                    RequiresBalance = false, // Bakiye kontrolü yok
                    DeductsFromBalance = false, // Yıllık izinden düşmez
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Raporlu İzin", 
                    Description = "Sağlık sorunları için raporlu izin",
                    MaxDaysPerYear = 0, // Sınırsız
                    RequiresApproval = true,
                    IsPaid = true,
                    RequiresBalance = false, // Bakiye kontrolü yok
                    DeductsFromBalance = false, // Yıllık izinden düşmez
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Ücretsiz İzin", 
                    Description = "Ücret kesilmesi ile kullanılan izin",
                    MaxDaysPerYear = 0, // Sınırsız
                    RequiresApproval = true,
                    IsPaid = false,
                    RequiresBalance = false, // Bakiye kontrolü yok
                    DeductsFromBalance = false, // Bakiyeden düşmez
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };
            await context.LeaveTypes.AddRangeAsync(leaveTypes);
            await context.SaveChangesAsync();

            // Initialize official holidays for current year and next year
            await InitializeOfficialHolidaysAsync(context);
        }

        public static async Task ForceInitializeAsync(LeaveManagementDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create Titles (always recreate)
            var titles = new[]
            {
                new Title { Id = 1, Name = "Yönetici", Description = "Yönetici", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 2, Name = "Uzman", Description = "Uzman", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 3, Name = "Uzman Yardımcısı", Description = "Uzman Yardımcısı", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 4, Name = "Takım Lideri Operatör", Description = "Takım Lideri Operatör", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 5, Name = "Güvenlik Personeli", Description = "Güvenlik Personeli", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 6, Name = "Şoför", Description = "Şoför", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 7, Name = "Direktör", Description = "Direktör", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 8, Name = "Ambalaj Şefi", Description = "Ambalaj Şefi", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 9, Name = "PLASTİKHANE ŞEFİ", Description = "PLASTİKHANE ŞEFİ", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 10, Name = "VARDİYA ŞEFİ", Description = "VARDİYA ŞEFİ", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 11, Name = "BAHÇIVAN", Description = "BAHÇIVAN", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Title { Id = 12, Name = "Personel", Description = "Personel", IsActive = true, CreatedDate = DateTime.UtcNow }
            };
            await context.Titles.AddRangeAsync(titles);
            await context.SaveChangesAsync();

            // Always check and create/update admin
            var admin = await context.Employees.FirstOrDefaultAsync(e => e.Username == "admin");
            if (admin == null)
            {
                await CreateAdminUserAsync(context);
            }
            else if (!admin.IsSystemAdmin)
            {
                // Update existing admin to be system admin and remove from department
                admin.IsSystemAdmin = true;
                admin.DepartmentId = null;
                context.Employees.Update(admin);
                await context.SaveChangesAsync();
            }

            // Create Default Leave Types (always recreate)
            var leaveTypes = new[]
            {
                new LeaveType 
                { 
                    Name = "Yıllık İzin", 
                    Description = "Yıllık ücretli izin hakkı",
                    MaxDaysPerYear = 14,
                    RequiresApproval = true,
                    IsPaid = true,
                    RequiresBalance = true,
                    DeductsFromBalance = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Mazeret İzni", 
                    Description = "Doğum, ölüm, evlilik vb. durumlar için mazeret izni",
                    MaxDaysPerYear = 0,
                    RequiresApproval = true,
                    IsPaid = true,
                    RequiresBalance = false,
                    DeductsFromBalance = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Raporlu İzin", 
                    Description = "Sağlık sorunları için raporlu izin",
                    MaxDaysPerYear = 0,
                    RequiresApproval = true,
                    IsPaid = true,
                    RequiresBalance = false,
                    DeductsFromBalance = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new LeaveType 
                { 
                    Name = "Ücretsiz İzin", 
                    Description = "Ücret kesilmesi ile kullanılan izin",
                    MaxDaysPerYear = 0,
                    RequiresApproval = true,
                    IsPaid = false,
                    RequiresBalance = false,
                    DeductsFromBalance = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };
            await context.LeaveTypes.AddRangeAsync(leaveTypes);
            await context.SaveChangesAsync();

            // Initialize official holidays for current year and next year
            await InitializeOfficialHolidaysAsync(context);
        }

        private static async Task InitializeOfficialHolidaysAsync(LeaveManagementDbContext context)
        {
            var currentYear = DateTime.Now.Year;
            var nextYear = currentYear + 1;

            // Check and create holidays for current year
            if (!await context.Holidays.AnyAsync(h => h.Year == currentYear && h.IsActive))
            {
                await CreateOfficialHolidaysForYearAsync(context, currentYear);
            }

            // Check and create holidays for next year
            if (!await context.Holidays.AnyAsync(h => h.Year == nextYear && h.IsActive))
            {
                await CreateOfficialHolidaysForYearAsync(context, nextYear);
            }
        }

        private static async Task CreateOfficialHolidaysForYearAsync(LeaveManagementDbContext context, int year)
        {
            var holidays = new List<Holiday>();

            // Sabit resmi tatiller
            holidays.Add(new Holiday { Name = "Yılbaşı", Date = new DateTime(year, 1, 1), Year = year, Description = "Yılbaşı Tatili", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Ulusal Egemenlik ve Çocuk Bayramı", Date = new DateTime(year, 4, 23), Year = year, Description = "23 Nisan Ulusal Egemenlik ve Çocuk Bayramı", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Emek ve Dayanışma Günü", Date = new DateTime(year, 5, 1), Year = year, Description = "1 Mayıs Emek ve Dayanışma Günü", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Atatürk'ü Anma, Gençlik ve Spor Bayramı", Date = new DateTime(year, 5, 19), Year = year, Description = "19 Mayıs Atatürk'ü Anma, Gençlik ve Spor Bayramı", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Demokrasi ve Milli Birlik Günü", Date = new DateTime(year, 7, 15), Year = year, Description = "15 Temmuz Demokrasi ve Milli Birlik Günü", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Zafer Bayramı", Date = new DateTime(year, 8, 30), Year = year, Description = "30 Ağustos Zafer Bayramı", IsActive = true, CreatedDate = DateTime.UtcNow });
            holidays.Add(new Holiday { Name = "Cumhuriyet Bayramı", Date = new DateTime(year, 10, 29), Year = year, Description = "29 Ekim Cumhuriyet Bayramı", IsActive = true, CreatedDate = DateTime.UtcNow });

            // Dini bayramlar
            var religiousHolidayDates = new Dictionary<int, (DateTime ramadanStart, DateTime ramadanEnd, DateTime kurbanStart, DateTime kurbanEnd)>
            {
                { 2020, (new DateTime(2020, 5, 24), new DateTime(2020, 5, 26), new DateTime(2020, 7, 31), new DateTime(2020, 8, 3)) },
                { 2021, (new DateTime(2021, 5, 13), new DateTime(2021, 5, 15), new DateTime(2021, 7, 20), new DateTime(2021, 7, 23)) },
                { 2022, (new DateTime(2022, 5, 2), new DateTime(2022, 5, 4), new DateTime(2022, 7, 9), new DateTime(2022, 7, 12)) },
                { 2023, (new DateTime(2023, 4, 21), new DateTime(2023, 4, 23), new DateTime(2023, 6, 28), new DateTime(2023, 7, 1)) },
                { 2024, (new DateTime(2024, 4, 10), new DateTime(2024, 4, 12), new DateTime(2024, 6, 16), new DateTime(2024, 6, 19)) },
                { 2025, (new DateTime(2025, 3, 30), new DateTime(2025, 4, 1), new DateTime(2025, 6, 6), new DateTime(2025, 6, 9)) },
                { 2026, (new DateTime(2026, 3, 20), new DateTime(2026, 3, 22), new DateTime(2026, 5, 26), new DateTime(2026, 5, 29)) },
                { 2027, (new DateTime(2027, 3, 9), new DateTime(2027, 3, 11), new DateTime(2027, 5, 16), new DateTime(2027, 5, 19)) },
                { 2028, (new DateTime(2028, 2, 26), new DateTime(2028, 2, 28), new DateTime(2028, 5, 4), new DateTime(2028, 5, 7)) },
                { 2029, (new DateTime(2029, 2, 14), new DateTime(2029, 2, 16), new DateTime(2029, 4, 23), new DateTime(2029, 4, 26)) },
                { 2030, (new DateTime(2030, 2, 4), new DateTime(2030, 2, 6), new DateTime(2030, 4, 13), new DateTime(2030, 4, 16)) }
            };

            if (religiousHolidayDates.ContainsKey(year))
            {
                var dates = religiousHolidayDates[year];
                // Ramazan Bayramı (3 gün)
                for (int i = 0; i < 3; i++)
                {
                    var date = dates.ramadanStart.AddDays(i);
                    holidays.Add(new Holiday
                    {
                        Name = i == 0 ? "Ramazan Bayramı (1. Gün)" : i == 1 ? "Ramazan Bayramı (2. Gün)" : "Ramazan Bayramı (3. Gün)",
                        Date = date,
                        Year = year,
                        Description = "Ramazan Bayramı",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
                // Kurban Bayramı (4 gün)
                for (int i = 0; i < 4; i++)
                {
                    var date = dates.kurbanStart.AddDays(i);
                    holidays.Add(new Holiday
                    {
                        Name = i == 0 ? "Kurban Bayramı (1. Gün)" : i == 1 ? "Kurban Bayramı (2. Gün)" : i == 2 ? "Kurban Bayramı (3. Gün)" : "Kurban Bayramı (4. Gün)",
                        Date = date,
                        Year = year,
                        Description = "Kurban Bayramı",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await context.Holidays.AddRangeAsync(holidays);
            await context.SaveChangesAsync();
        }

        private static async Task CreateAdminUserAsync(LeaveManagementDbContext context)
        {
            // Get HR Department
            var hrDepartment = await context.Departments
                .FirstOrDefaultAsync(d => d.Name.Contains("İnsan Kaynakları") || d.Name.Contains("Human Resources"));
            
            if (hrDepartment == null)
            {
                // Create HR Department if it doesn't exist
                hrDepartment = new Department
                {
                    Name = "İnsan Kaynakları",
                    Description = "İnsan Kaynakları Departmanı",
                    Code = "HR",
                    IsActive = true,
                    IsSystem = true,
                    CreatedDate = DateTime.UtcNow
                };
                await context.Departments.AddAsync(hrDepartment);
                await context.SaveChangesAsync();
            }

            // Get Yönetici title
            var yoneticiTitle = await context.Titles.FirstOrDefaultAsync(t => t.Name == "Yönetici");
            if (yoneticiTitle == null)
            {
                // Create Yönetici title if it doesn't exist
                yoneticiTitle = new Title
                {
                    Name = "Yönetici",
                    Description = "Yönetici",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await context.Titles.AddAsync(yoneticiTitle);
                await context.SaveChangesAsync();
            }

            // Create System Admin Employee
            // System admin is not part of any department and not shown in employee lists
            var adminEmployee = new Employee
            {
                FirstName = "Sistem",
                LastName = "Admin",
                Email = "admin@company.com",
                EmployeeNumber = "SYS001",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                TitleId = yoneticiTitle.Id,
                DepartmentId = null, // System admin is not part of any department
                IsActive = true,
                IsSystemAdmin = true, // Mark as system admin
                HireDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();
            
            // Note: HR Department manager should be set to a regular employee, not system admin
            // System admin is separate from department structure
        }
    }
}
