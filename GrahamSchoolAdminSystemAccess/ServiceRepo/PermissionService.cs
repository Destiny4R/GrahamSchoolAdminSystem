using GrahamSchoolAdminSystemAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    /// <summary>
    /// Service for checking user permissions based on their assigned roles
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permissionName))
                return false;

            // Get all roles for the user
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!userRoleIds.Any())
                return false;

            // Check if any of the user's roles have the requested permission
            var hasPermission = await _context.RolePermissions
                .AnyAsync(rp => userRoleIds.Contains(rp.RoleId) && rp.Permission.Name == permissionName);

            return hasPermission;
        }

        /// <summary>
        /// Check if a user has a specific permission (using ClaimsPrincipal)
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal user, string permissionName)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            return await UserHasPermissionAsync(userId, permissionName);
        }

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        /// <summary>
        /// Get all permissions for a user (using ClaimsPrincipal)
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            return await GetUserPermissionsAsync(userId);
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return roles ?? new List<string>();
        }

        /// <summary>
        /// Check if user has any of the specified permissions
        /// </summary>
        public async Task<bool> UserHasAnyPermissionAsync(string userId, params string[] permissionNames)
        {
            if (string.IsNullOrEmpty(userId) || permissionNames == null || !permissionNames.Any())
                return false;

            foreach (var permission in permissionNames)
            {
                if (await UserHasPermissionAsync(userId, permission))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        public async Task<bool> UserHasAllPermissionsAsync(string userId, params string[] permissionNames)
        {
            if (string.IsNullOrEmpty(userId) || permissionNames == null || !permissionNames.Any())
                return false;

            foreach (var permission in permissionNames)
            {
                if (!await UserHasPermissionAsync(userId, permission))
                    return false;
            }

            return true;
        }
    }
}
