using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.Entity
{
    public class LeaveBalance
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;
        
        public int Year { get; set; }
        
        public int TotalDays { get; set; }
        
        public int UsedDays { get; set; } = 0;
        
        public int RemainingDays => TotalDays - UsedDays;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}


