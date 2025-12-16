using LeaveManagement.Business.Models;

namespace LeaveManagement.Business.Interfaces
{
    public interface IHolidayService
    {
        Task<IEnumerable<HolidayDto>> GetAllHolidaysAsync();
        Task<IEnumerable<HolidayDto>> GetHolidaysByYearAsync(int year);
        Task<HolidayDto?> GetHolidayByIdAsync(int id);
        Task<HolidayDto> CreateHolidayAsync(CreateHolidayDto createDto);
        Task<bool> UpdateHolidayAsync(int id, UpdateHolidayDto updateDto);
        Task<bool> DeleteHolidayAsync(int id);
        Task<IEnumerable<HolidayDto>> GenerateOfficialHolidaysForYearAsync(int year);
        Task<bool> EnsureHolidaysForYearAsync(int year);
    }
}

