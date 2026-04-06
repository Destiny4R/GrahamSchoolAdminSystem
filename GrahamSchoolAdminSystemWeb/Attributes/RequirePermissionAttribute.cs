using GrahamSchoolAdminSystemAccess.ServiceRepo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Attributes
{
    /// <summary>
    /// Custom authorization attribute that checks if user has required permission(s)
    /// Usage: [RequirePermission("Create")] or [RequirePermission("Create", "Edit")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;
        private readonly bool _requireAll;

        /// <summary>
        /// Creates a permission requirement
        /// </summary>
        /// <param name="permissions">Required permission(s)</param>
        /// <param name="requireAll">If true, user must have ALL permissions. If false, user needs ANY permission.</param>
        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _requireAll = true; // Default: require all permissions
        }

        /// <summary>
        /// Creates a permission requirement with control over AND/OR logic
        /// </summary>
        public RequirePermissionAttribute(bool requireAll, params string[] permissions)
        {
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            _requireAll = requireAll;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToPageResult("/account/login");
                return;
            }

            // Get PermissionService
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

            bool hasPermission;

            if (_requireAll)
            {
                // User must have ALL permissions
                hasPermission = await permissionService.UserHasAllPermissionsAsync(userId, _permissions);
            }
            else
            {
                // User must have ANY of the permissions
                hasPermission = await permissionService.UserHasAnyPermissionAsync(userId, _permissions);
            }

            if (!hasPermission)
            {
                context.Result = new RedirectToPageResult("/account/accessdenied");
            }
        }
    }
}
