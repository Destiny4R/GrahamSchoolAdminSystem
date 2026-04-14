using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Helpers
{
    /// <summary>
    /// Scoped service that caches the current user's permissions for the request lifetime.
    /// Inject into Razor views via @inject to conditionally show/hide UI elements.
    /// </summary>
    public class ViewPermissionService
    {
        private readonly IPermissionService _permissionService;
        private List<string>? _permissions;
        private bool _initialized;

        public ViewPermissionService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public bool CanCreate { get; private set; }
        public bool CanEdit { get; private set; }
        public bool CanDelete { get; private set; }
        public bool CanView { get; private set; }
        public bool CanReport { get; private set; }

        /// <summary>
        /// Loads permissions for the current user. Safe to call multiple times — only queries DB once.
        /// </summary>
        public async Task InitializeAsync(ClaimsPrincipal user)
        {
            if (_initialized) return;
            _initialized = true;

            _permissions = await _permissionService.GetUserPermissionsAsync(user);

            CanCreate = _permissions.Contains(SD.Permissions.CREATE);
            CanEdit = _permissions.Contains(SD.Permissions.EDIT);
            CanDelete = _permissions.Contains(SD.Permissions.DELETE);
            CanView = _permissions.Contains(SD.Permissions.VIEW);
            CanReport = _permissions.Contains(SD.Permissions.REPORT);
        }
    }
}
