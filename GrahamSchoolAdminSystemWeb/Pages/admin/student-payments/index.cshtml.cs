using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.student_payments
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        public indexModel(IUnitOfWork unitOfWork, ILogger<indexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
