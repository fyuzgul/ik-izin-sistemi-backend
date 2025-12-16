using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class GoalType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        public List<GoalCardItem> GoalCardItems { get; set; } = new List<GoalCardItem>();
    }
}

