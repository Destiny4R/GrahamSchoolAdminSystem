using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.student_payments
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class full_receiptModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConsolidatedReceiptViewModel Receipt { get; set; }

        public full_receiptModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int? termRegId)
        {
            if (!termRegId.HasValue || termRegId.Value <= 0)
            {
                return RedirectToPage("/admin/student-payments/index");
            }

            Receipt = await _unitOfWork.StudentPaymentService.GetConsolidatedReceiptAsync(termRegId.Value);
            if (Receipt == null || !Receipt.Categories.Any())
            {
                return RedirectToPage("/admin/student-payments/index");
            }

            return Page();
        }
    }
}
