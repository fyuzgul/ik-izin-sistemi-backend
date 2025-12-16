using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Business.Models
{
    public class LoginDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
    }
    
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
    }
    
    public class CreateEmployeeDto
    {
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
        
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        
        public int? DepartmentId { get; set; }
        
        [StringLength(50)]
        public string? Username { get; set; }
        
        [StringLength(255)]
        public string? Password { get; set; }
        
        public int? TitleId { get; set; }
        
        public bool WorksOnSaturday { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
    }
    
    public class UpdateEmployeeDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public int? DepartmentId { get; set; }
        
        [StringLength(50)]
        public string? Username { get; set; }
        
        [StringLength(255)]
        public string? Password { get; set; }
        
        public int? TitleId { get; set; }
        
        public bool WorksOnSaturday { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
    }
    
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime HireDate { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? TitleId { get; set; }
        public string TitleName { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool WorksOnSaturday { get; set; } = false;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}