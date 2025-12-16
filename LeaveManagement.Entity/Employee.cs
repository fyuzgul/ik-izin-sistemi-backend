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
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        // Kullanıcı bilgileri
        [StringLength(50)]
        public string? Username { get; set; }
        
        [StringLength(255)]
        public string? PasswordHash { get; set; }
        
        public int? TitleId { get; set; }
        public Title? Title { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
        
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }
        
        public List<Employee> Subordinates { get; set; } = new List<Employee>();
        
        public List<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        
        public List<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        
        // Indicates if employee works on Saturdays (used for leave day calculation)
        public bool WorksOnSaturday { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        // System admin flag - system admin is not part of any department and not shown in employee lists
        public bool IsSystemAdmin { get; set; } = false;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}


