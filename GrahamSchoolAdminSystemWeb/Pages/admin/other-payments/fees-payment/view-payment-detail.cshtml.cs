using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.fees_payment
{
    public class view_payment_detailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentDetailViewModel PaymentDetail { get; set; }

        public view_payment_detailModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                PaymentDetail = await _unitOfWork.OtherPaymentServices.GetOtherPaymentDetailAsync(id);

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
                var result = await _unitOfWork.OtherPaymentServices.ApproveOtherPaymentAsync(paymentId);

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
                var result = await _unitOfWork.OtherPaymentServices.RejectOtherPaymentAsync(paymentId);

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
                var result = await _unitOfWork.OtherPaymentServices.CancelOtherPaymentAsync(paymentId);

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
                TempData["Error"] = "An error occurred while cancelling the payment.";
            }

            return RedirectToPage("view-payment-detail", new { id = paymentId });
        }
    }
}
