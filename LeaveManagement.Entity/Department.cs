using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class Department
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string? Code { get; set; }
        
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        
        public List<Employee> Employees { get; set; } = new List<Employee>();
        
        public bool IsActive { get; set; } = true;
        
        // System protected - cannot be deleted or modified
        public bool IsSystem { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}


