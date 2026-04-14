using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.payment_reports
{
    [Authorize]
    [RequirePermission(SD.Permissions.REPORT)]
    public class category_reportModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public category_reportModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }
    }
}
