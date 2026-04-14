using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.roles
{
    /// <summary>
    /// Example page demonstrating permission-based authorization
    /// This page requires users to have the "View" permission
    /// </summary>
    [Authorize]
    [RequireRole(SD.Roles.ADMIN)]
    [RequirePermission("View")]
    public class PermissionsExampleModel : PageModel
    {
        public string CurrentUserName { get; set; } = string.Empty;
        public List<string> Examples { get; set; } = new List<string>();

        public void OnGet()
        {
            CurrentUserName = User.Identity?.Name ?? "Guest";
            
            Examples = new List<string>
            {
                "[RequirePermission(\"Create\")] - Requires Create permission",
                "[RequirePermission(\"Edit\")] - Requires Edit permission",
                "[RequirePermission(\"Delete\")] - Requires Delete permission",
                "[RequirePermission(\"View\")] - Requires View permission",
                "[RequirePermission(\"Create\", \"Edit\")] - Requires both Create AND Edit",
                "[RequirePermission(false, \"Create\", \"Edit\")] - Requires Create OR Edit"
            };
        }
    }
}
