using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class GoalCardTemplate
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Departman ve pozisyon bazlı şablon
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        
        public int TitleId { get; set; }
        public Title Title { get; set; } = null!;
        
        // Şablonu oluşturan yönetici
        public int CreatedByEmployeeId { get; set; }
        public Employee CreatedByEmployee { get; set; } = null!;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        public List<GoalCardItem> Items { get; set; } = new List<GoalCardItem>();
        public List<EmployeeGoalCard> EmployeeGoalCards { get; set; } = new List<EmployeeGoalCard>();
    }
}

