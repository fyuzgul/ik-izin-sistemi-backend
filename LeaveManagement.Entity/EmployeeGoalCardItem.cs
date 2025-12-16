using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class EmployeeGoalCardItem
    {
        public int Id { get; set; }
        
        public int EmployeeGoalCardId { get; set; }
        public EmployeeGoalCard EmployeeGoalCard { get; set; } = null!;
        
        // Şablon maddesine referans
        public int GoalCardItemId { get; set; }
        public GoalCardItem GoalCardItem { get; set; } = null!;
        
        // Gerçekleşme tarihi
        public DateTime? ActualCompletionDate { get; set; }
        
        // Gerçekleşme yüzdesi (80, 100, 120 vb.)
        [StringLength(50)]
        public string? AchievementLevel { get; set; }
        
        // Yönetici notları
        [StringLength(2000)]
        public string? ManagerNotes { get; set; }
        
        // Çalışan notları
        [StringLength(2000)]
        public string? EmployeeNotes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}

