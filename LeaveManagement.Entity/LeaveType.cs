using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class LeaveType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int MaxDaysPerYear { get; set; }
        
        public bool RequiresApproval { get; set; } = true;
        
        public bool IsPaid { get; set; } = true;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        public List<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        
        public List<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    }
}


