namespace LeaveManagement.Entity
{
    public static class UserRole
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string HrManager = "HrManager";
        public const string DepartmentManager = "DepartmentManager";
        public const string Employee = "Employee";
        
        public static readonly Dictionary<string, RolePermissions> RolePermissionsMap = new()
        {
            [SystemAdmin] = new RolePermissions
            {
                CanManageEmployees = true,
                CanManageDepartments = true,
                CanManageLeaveTypes = true,
                CanApproveLeaveRequests = true,
                CanViewAllLeaveRequests = true,
                CanManageSystemSettings = true
            },
            [HrManager] = new RolePermissions
            {
                CanManageEmployees = true,
                CanManageDepartments = false,
                CanManageLeaveTypes = true,
                CanApproveLeaveRequests = true,
                CanViewAllLeaveRequests = true,
                CanManageSystemSettings = false
            },
            [DepartmentManager] = new RolePermissions
            {
                CanManageEmployees = false,
                CanManageDepartments = false,
                CanManageLeaveTypes = false,
                CanApproveLeaveRequests = true,
                CanViewAllLeaveRequests = false,
                CanManageSystemSettings = false
            },
            [Employee] = new RolePermissions
            {
                CanManageEmployees = false,
                CanManageDepartments = false,
                CanManageLeaveTypes = false,
                CanApproveLeaveRequests = false,
                CanViewAllLeaveRequests = false,
                CanManageSystemSettings = false
            }
        };
    }
    
    public class RolePermissions
    {
        public bool CanManageEmployees { get; set; }
        public bool CanManageDepartments { get; set; }
        public bool CanManageLeaveTypes { get; set; }
        public bool CanApproveLeaveRequests { get; set; }
        public bool CanViewAllLeaveRequests { get; set; }
        public bool CanManageSystemSettings { get; set; }
    }
}
