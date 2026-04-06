using System.Security.Claims;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
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
    }
}
