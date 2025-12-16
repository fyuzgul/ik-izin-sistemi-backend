namespace LeaveManagement.Business.Models
{
    public class HolidayDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateHolidayDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateHolidayDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}

