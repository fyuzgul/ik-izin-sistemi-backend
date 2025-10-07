using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.Entity
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int TotalDays { get; set; }
        
        [StringLength(1000)]
        public string? Reason { get; set; }
        
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
        
        public int? DepartmentManagerId { get; set; }
        public Employee? DepartmentManager { get; set; }
        
        public int? HrManagerId { get; set; }
        public Employee? HrManager { get; set; }
        
        public DateTime? DepartmentManagerApprovalDate { get; set; }
        public DateTime? HrManagerApprovalDate { get; set; }
        
        [StringLength(500)]
        public string? DepartmentManagerComments { get; set; }
        
        [StringLength(500)]
        public string? HrManagerComments { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
    
    public enum LeaveRequestStatus
    {
        Pending = 0,
        ApprovedByDepartmentManager = 1,
        ApprovedByHrManager = 2,
        RejectedByDepartmentManager = 3,
        RejectedByHrManager = 4,
        Cancelled = 5
    }
}


