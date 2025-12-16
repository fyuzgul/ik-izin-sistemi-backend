using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class EmployeeGoalCard
    {
        public int Id { get; set; }
        
        // Hangi çalışan için
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        // Hangi şablondan oluşturuldu
        public int GoalCardTemplateId { get; set; }
        public GoalCardTemplate GoalCardTemplate { get; set; } = null!;
        
        // Hangi yönetici tarafından dolduruldu
        public int CreatedByEmployeeId { get; set; }
        public Employee CreatedByEmployee { get; set; } = null!;
        
        // Dönem bilgisi (yıl)
        public int Year { get; set; }
        
        // Durum
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Active, Completed, Cancelled
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        public List<EmployeeGoalCardItem> Items { get; set; } = new List<EmployeeGoalCardItem>();
    }
}

