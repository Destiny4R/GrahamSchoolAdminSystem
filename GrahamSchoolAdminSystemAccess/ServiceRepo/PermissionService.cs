using GrahamSchoolAdminSystemAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    /// <summary>
    /// Service for checking user permissions based on Position-based authorization.
    /// Authorization chain: Employee → Position → PositionRole → Role → RolePermission → Permission
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Check if a user has a specific permission via Employee → Position → Role → Permission chain
        /// </summary>
        public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permissionName))
                return false;

            // Principal position always has all permissions
            var isPrincipal = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .AnyAsync(e => e.Position.Name == SD.Positions.PRINCIPAL);
            if (isPrincipal)
                return true;

            // Find the employee's position and check if any role on that position has the permission
            var hasPermission = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .SelectMany(e => e.Position.PositionRoles)
                .SelectMany(pr => pr.Role.RolePermissions)
                .AnyAsync(rp => rp.Permission.Name == permissionName);

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
        /// Get all permissions for a user via Employee → Position → Role → Permission chain
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            // Principal position always has all permissions
            var isPrincipal = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .AnyAsync(e => e.Position.Name == SD.Positions.PRINCIPAL);
            if (isPrincipal)
                return SD.GetAllPermissions();

            var permissions = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .SelectMany(e => e.Position.PositionRoles)
                .SelectMany(pr => pr.Role.RolePermissions)
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
        /// Get all roles for a user via Employee → Position → PositionRole chain
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            // Principal position always has the Admin role
            var isPrincipal = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .AnyAsync(e => e.Position.Name == SD.Positions.PRINCIPAL);
            if (isPrincipal)
                return new List<string> { SD.Roles.ADMIN };

            var roles = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .SelectMany(e => e.Position.PositionRoles)
                .Select(pr => pr.Role.Name)
                .Distinct()
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

            // Principal position always has all permissions
            var isPrincipal = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .AnyAsync(e => e.Position.Name == SD.Positions.PRINCIPAL);
            if (isPrincipal)
                return true;

            var hasAny = await _context.Employees
                .Where(e => e.ApplicationUserId == userId && e.PositionId != null)
                .SelectMany(e => e.Position.PositionRoles)
                .SelectMany(pr => pr.Role.RolePermissions)
                .AnyAsync(rp => permissionNames.Contains(rp.Permission.Name));

            return hasAny;
        }

        /// <summary>
        /// Check if user has all of the specified permissions
        /// </summary>
        public async Task<bool> UserHasAllPermissionsAsync(string userId, params string[] permissionNames)
        {
            if (string.IsNullOrEmpty(userId) || permissionNames == null || !permissionNames.Any())
                return false;

            var userPermissions = await GetUserPermissionsAsync(userId);
            return permissionNames.All(p => userPermissions.Contains(p));
        }

        /// <summary>
        /// Get display info (name, position, roles) via Employee → Position → Role chain
        /// </summary>
        public async Task<UserDisplayInfo> GetUserDisplayInfoAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new UserDisplayInfo();

            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.PositionRoles)
                        .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId);

            if (employee == null)
                return new UserDisplayInfo();

            return new UserDisplayInfo
            {
                FullName = employee.FullName ?? "User",
                PositionName = employee.Position?.Name ?? "",
                Roles = employee.Position?.PositionRoles?
                    .Select(pr => pr.Role?.Name)
                    .Where(n => n != null)
                    .ToList() ?? new List<string>()
            };
        }
    }
}
