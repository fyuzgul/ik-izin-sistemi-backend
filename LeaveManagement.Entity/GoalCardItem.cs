using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.Entity
{
    public class GoalCardItem
    {
        public int Id { get; set; }
        
        public int GoalCardTemplateId { get; set; }
        public GoalCardTemplate GoalCardTemplate { get; set; } = null!;
        
        public int GoalTypeId { get; set; }
        public GoalType GoalType { get; set; } = null!;
        
        [Required]
        [StringLength(500)]
        public string Goal { get; set; } = string.Empty;
        
        public DateTime? TargetDate { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // Ağırlık (yüzde olarak)
        
        [StringLength(200)]
        public string? Target80Percent { get; set; }
        
        [StringLength(200)]
        public string? Target100Percent { get; set; }
        
        [StringLength(200)]
        public string? Target120Percent { get; set; }
        
        [StringLength(1000)]
        public string? GoalDescription { get; set; }
        
        public int Order { get; set; } // Sıralama
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        public List<EmployeeGoalCardItem> EmployeeGoalCardItems { get; set; } = new List<EmployeeGoalCardItem>();
    }
}

