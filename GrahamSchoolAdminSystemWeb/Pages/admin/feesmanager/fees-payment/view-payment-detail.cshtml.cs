using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.feesmanager.fees_payment
{
    public class ViewPaymentDetailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentDetailViewModel PaymentDetail { get; set; }

        public ViewPaymentDetailModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                PaymentDetail = await _unitOfWork.FeesPaymentServices.GetPaymentDetailAsync(id);

                if (PaymentDetail == null)
                {
                    TempData["Error"] = "Payment not found.";
                    return RedirectToPage("index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading payment details.";
                return RedirectToPage("index");
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int paymentId)
        {
            try
            {
                var result = await _unitOfWork.FeesPaymentServices.ApprovePaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Payment approved successfully.";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while approving the payment.";
            }

            return RedirectToPage("view-payment-detail", new { id = paymentId });
        }

        public async Task<IActionResult> OnPostRejectAsync(int paymentId)
        {
            try
            {
                var result = await _unitOfWork.FeesPaymentServices.RejectPaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Payment rejected successfully.";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while rejecting the payment.";
            }

            return RedirectToPage("view-payment-detail", new { id = paymentId });
        }

        public async Task<IActionResult> OnPostCancelAsync(int paymentId)
        {
            try
            {
                var result = await _unitOfWork.FeesPaymentServices.CancelPaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Payment cancelled successfully.";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while canceling the payment.";
            }

            return RedirectToPage("view-payment-detail", new { id = paymentId });
        }
    }
}
