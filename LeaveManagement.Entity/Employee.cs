using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;
        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        
        public List<Employee> Subordinates { get; set; } = new List<Employee>();
        
        public List<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        
        public List<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}


