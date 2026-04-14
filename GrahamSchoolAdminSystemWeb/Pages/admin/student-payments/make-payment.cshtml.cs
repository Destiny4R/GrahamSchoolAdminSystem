using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.student_payments
{
    [Authorize]
    [RequirePermission(SD.Permissions.CREATE)]
    public class make_paymentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public bool EvidenceRequired { get; set; }

        public make_paymentModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnGetAsync()
        {
            var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
            EvidenceRequired = settings?.PaymentEvidence ?? true;
        }
    }
}
