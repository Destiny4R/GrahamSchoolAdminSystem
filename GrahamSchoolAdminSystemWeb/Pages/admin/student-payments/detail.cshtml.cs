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
    public class detailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<detailModel> _logger;

        public PaymentReceiptViewModel Payment { get; set; }

        public detailModel(IUnitOfWork unitOfWork, ILogger<detailModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                TempData["Error"] = "Invalid payment ID.";
                return RedirectToPage("/admin/student-payments/index");
            }

            Payment = await _unitOfWork.StudentPaymentService.GetPaymentDetailAsync(id.Value);
            if (Payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToPage("/admin/student-payments/index");
            }

            return Page();
        }
    }
}
