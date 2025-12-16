using LeaveManagement.Business.Interfaces;
using LeaveManagement.Business.Models;
using LeaveManagement.DataAccess;
using LeaveManagement.Entity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Business.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LeaveManagementDbContext _context;

        public HolidayService(IUnitOfWork unitOfWork, LeaveManagementDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IEnumerable<HolidayDto>> GetAllHolidaysAsync()
        {
            var holidays = await _unitOfWork.Holidays.FindAsync(h => h.IsActive);
            return holidays.OrderBy(h => h.Date).Select(MapToDto);
        }

        public async Task<IEnumerable<HolidayDto>> GetHolidaysByYearAsync(int year)
        {
            var holidays = await _context.Holidays
                .Where(h => h.IsActive && h.Year == year)
                .OrderBy(h => h.Date)
                .ToListAsync();
            return holidays.Select(MapToDto);
        }

        public async Task<HolidayDto?> GetHolidayByIdAsync(int id)
        {
            var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
            return holiday != null ? MapToDto(holiday) : null;
        }

        public async Task<HolidayDto> CreateHolidayAsync(CreateHolidayDto createDto)
        {
            var holiday = new Holiday
            {
                Name = createDto.Name,
                Date = createDto.Date.Date,
                Year = createDto.Date.Year,
                Description = createDto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Holidays.AddAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(holiday);
        }

        public async Task<bool> UpdateHolidayAsync(int id, UpdateHolidayDto updateDto)
        {
            var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
            if (holiday == null)
                return false;

            holiday.Name = updateDto.Name;
            holiday.Date = updateDto.Date.Date;
            holiday.Year = updateDto.Date.Year;
            holiday.Description = updateDto.Description;
            holiday.IsActive = updateDto.IsActive;
            holiday.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Holidays.UpdateAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var holiday = await _unitOfWork.Holidays.GetByIdAsync(id);
            if (holiday == null)
                return false;

            // Soft delete
            holiday.IsActive = false;
            holiday.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Holidays.UpdateAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<HolidayDto>> GenerateOfficialHolidaysForYearAsync(int year)
        {
            var holidays = new List<Holiday>();

            // Sabit resmi tatiller
            holidays.Add(new Holiday
            {
                Name = "Yılbaşı",
                Date = new DateTime(year, 1, 1),
                Year = year,
                Description = "Yılbaşı Tatili",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Ulusal Egemenlik ve Çocuk Bayramı",
                Date = new DateTime(year, 4, 23),
                Year = year,
                Description = "23 Nisan Ulusal Egemenlik ve Çocuk Bayramı",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Emek ve Dayanışma Günü",
                Date = new DateTime(year, 5, 1),
                Year = year,
                Description = "1 Mayıs Emek ve Dayanışma Günü",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Atatürk'ü Anma, Gençlik ve Spor Bayramı",
                Date = new DateTime(year, 5, 19),
                Year = year,
                Description = "19 Mayıs Atatürk'ü Anma, Gençlik ve Spor Bayramı",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Demokrasi ve Milli Birlik Günü",
                Date = new DateTime(year, 7, 15),
                Year = year,
                Description = "15 Temmuz Demokrasi ve Milli Birlik Günü",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Zafer Bayramı",
                Date = new DateTime(year, 8, 30),
                Year = year,
                Description = "30 Ağustos Zafer Bayramı",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            holidays.Add(new Holiday
            {
                Name = "Cumhuriyet Bayramı",
                Date = new DateTime(year, 10, 29),
                Year = year,
                Description = "29 Ekim Cumhuriyet Bayramı",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            // Dini bayramlar (2020-2030 arası önceden tanımlanmış tarihler)
            var religiousHolidays = GetReligiousHolidays(year);
            holidays.AddRange(religiousHolidays);

            return holidays.Select(MapToDto);
        }

        public async Task<bool> EnsureHolidaysForYearAsync(int year)
        {
            // Check if holidays already exist for this year
            var existingHolidays = await _context.Holidays
                .Where(h => h.Year == year && h.IsActive)
                .ToListAsync();

            if (existingHolidays.Any())
            {
                return false; // Holidays already exist
            }

            // Generate holidays for the year
            var holidays = await GenerateOfficialHolidaysForYearAsync(year);

            // Add to database
            foreach (var holidayDto in holidays)
            {
                var holiday = new Holiday
                {
                    Name = holidayDto.Name,
                    Date = holidayDto.Date,
                    Year = holidayDto.Year,
                    Description = holidayDto.Description,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Holidays.AddAsync(holiday);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private List<Holiday> GetReligiousHolidays(int year)
        {
            var holidays = new List<Holiday>();

            // Dini bayram tarihleri (2020-2030 arası)
            // Bu tarihler Diyanet İşleri Başkanlığı'ndan alınabilir
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

            return holidays;
        }

        private HolidayDto MapToDto(Holiday holiday)
        {
            return new HolidayDto
            {
                Id = holiday.Id,
                Name = holiday.Name,
                Date = holiday.Date,
                Year = holiday.Year,
                Description = holiday.Description,
                IsActive = holiday.IsActive,
                CreatedDate = holiday.CreatedDate
            };
        }
    }
}

