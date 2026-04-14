using GrahamSchoolAdminSystemAccess.ServiceRepo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Attributes
{
    /// <summary>
    /// Custom authorization attribute that checks if user has required role(s)
    /// via the Employee → Position → Role chain.
    /// Usage: [RequireRole("Admin")] or [RequireRole(false, "Admin", "Accountant", "Cashier")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly bool _requireAll;

        /// <summary>
        /// Creates a role requirement (requires ALL specified roles by default)
        /// </summary>
        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _requireAll = false; // Default: require ANY role
        }

        /// <summary>
        /// Creates a role requirement with control over AND/OR logic
        /// </summary>
        public RequireRoleAttribute(bool requireAll, params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _requireAll = requireAll;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToPageResult("/account/login");
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToPageResult("/account/login");
                return;
            }

            var userRoles = await permissionService.GetUserRolesAsync(userId);

            bool hasRole;
            if (_requireAll)
            {
                hasRole = _roles.All(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
            }
            else
            {
                hasRole = _roles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
            }

            if (!hasRole)
            {
                context.Result = new RedirectToPageResult("/account/accessdenied");
            }
        }
    }
}
