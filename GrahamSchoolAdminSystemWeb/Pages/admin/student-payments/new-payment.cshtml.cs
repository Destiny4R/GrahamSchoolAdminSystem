using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.student_payments
{
    [Authorize]
    [RequirePermission(SD.Permissions.CREATE)]
    public class new_paymentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public bool EvidenceRequired { get; set; }

        [BindProperty(SupportsGet = true)]
        public int TermRegId { get; set; }

        public new_paymentModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TermRegId <= 0)
            {
                TempData["Error"] = "Invalid term registration selected.";
                return RedirectToPage("/admin/termly-registeration/index");
            }

            var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
            EvidenceRequired = settings?.PaymentEvidence ?? true;
            return Page();
        }
    }
}
