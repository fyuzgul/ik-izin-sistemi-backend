using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Entity
{
    public class Role
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public bool CanManageEmployees { get; set; } = false;
        public bool CanManageDepartments { get; set; } = false;
        public bool CanManageLeaveTypes { get; set; } = false;
        public bool CanApproveLeaveRequests { get; set; } = false;
        public bool CanViewAllLeaveRequests { get; set; } = false;
        public bool CanManageSystemSettings { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        public List<User> Users { get; set; } = new List<User>();
    }
}
