using System.ComponentModel.DataAnnotations;
using LeaveManagement.Entity;

namespace LeaveManagement.Business.Models
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        
        [Required]
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int TotalDays { get; set; }
        
        public string? Reason { get; set; }
        
        public LeaveRequestStatus Status { get; set; }
        
        public int? DepartmentManagerId { get; set; }
        public string? DepartmentManagerName { get; set; }
        
        public int? HrManagerId { get; set; }
        public string? HrManagerName { get; set; }
        
        public DateTime? DepartmentManagerApprovalDate { get; set; }
        public DateTime? HrManagerApprovalDate { get; set; }
        
        public string? DepartmentManagerComments { get; set; }
        
        public string? HrManagerComments { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public int LeaveTypeId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string? Reason { get; set; }
    }

    public class UpdateLeaveRequestStatusDto
    {
        [Required]
        public LeaveRequestStatus Status { get; set; }
        
        public string? Comments { get; set; }
    }

    public class LeaveBalanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
    }
}


