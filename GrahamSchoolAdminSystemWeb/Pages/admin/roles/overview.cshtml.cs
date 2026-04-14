using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.roles
{
    [Authorize]
    [RequireRole(SD.Roles.ADMIN)]
    public class OverviewModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
