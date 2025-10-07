using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
