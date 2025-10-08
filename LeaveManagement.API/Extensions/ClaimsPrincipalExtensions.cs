using System.Security.Claims;

namespace LeaveManagement.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetEmployeeId(this ClaimsPrincipal user)
        {
            var employeeIdClaim = user.FindFirst("EmployeeId");
            if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int employeeId))
            {
                return employeeId;
            }
            throw new UnauthorizedAccessException("Employee ID not found in token");
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? throw new UnauthorizedAccessException("Role not found in token");
        }
    }
}

