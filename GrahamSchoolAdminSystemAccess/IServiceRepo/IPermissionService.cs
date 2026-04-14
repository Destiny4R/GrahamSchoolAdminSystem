using System.Security.Claims;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    /// <summary>
    /// Display info for the currently logged-in user, resolved via the
    /// Employee → Position → Role chain.
    /// </summary>
    public class UserDisplayInfo
    {
        public string FullName { get; set; } = "User";
        public string PositionName { get; set; } = "";
        public List<string> Roles { get; set; } = new();

        public string Initials => string.Join("",
            FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(2)
                    .Select(w => char.ToUpper(w[0])));

        public bool IsAdmin => Roles.Contains(SD.Roles.ADMIN);
        public bool IsAccountant => Roles.Contains(SD.Roles.ACCOUNT);
        public bool IsCashier => Roles.Contains(SD.Roles.CASHIER);
        public bool CanViewFinance => IsAdmin || IsAccountant || IsCashier;
        public bool CanManageHR => IsAdmin;
        public bool CanManageSecurity => IsAdmin;
    }

    /// <summary>
    /// Interface for permission checking service
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        Task<bool> UserHasPermissionAsync(string userId, string permissionName);

        /// <summary>
        /// Check if a user has a specific permission (using ClaimsPrincipal)
        /// </summary>
        Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionName);

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        Task<List<string>> GetUserPermissionsAsync(string userId);

        /// <summary>
        /// Get all permissions for a user (using ClaimsPrincipal)
        /// </summary>
        Task<List<string>> GetUserPermissionsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        Task<List<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        Task<bool> UserHasAnyPermissionAsync(string userId, params string[] permissionNames);

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        Task<bool> UserHasAllPermissionsAsync(string userId, params string[] permissionNames);

        /// <summary>
        /// Get display info (name, position, roles) for a user via the Employee → Position → Role chain
        /// </summary>
        Task<UserDisplayInfo> GetUserDisplayInfoAsync(string userId);
    }
}
