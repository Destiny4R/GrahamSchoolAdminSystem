using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.ptamanager.pta_payment
{
    public class ViewPTAPaymentDetailModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewPTAPaymentDetailModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PTAPaymentDetailViewModel PaymentDetail { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                PaymentDetail = await _unitOfWork.PTAPaymentServices.GetPTAPaymentDetailAsync(id);

                if (PaymentDetail == null)
                {
                    TempData["Error"] = "Payment record not found.";
                    return RedirectToPage("index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the payment details.";
                return RedirectToPage("index");
            }
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            try
            {
                var paymentId = int.Parse(Request.Form["paymentId"]);
                var result = await _unitOfWork.PTAPaymentServices.ApprovePTAPaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToPage(new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while approving the payment.";
                return RedirectToPage("index");
            }
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            try
            {
                var paymentId = int.Parse(Request.Form["paymentId"]);
                var result = await _unitOfWork.PTAPaymentServices.RejectPTAPaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }

                TempData["Error"] = result.Message;
                return RedirectToPage(new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while rejecting the payment.";
                return RedirectToPage("index");
            }
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            try
            {
                var paymentId = int.Parse(Request.Form["paymentId"]);
                var result = await _unitOfWork.PTAPaymentServices.CancelPTAPaymentAsync(paymentId);

                if (result.Succeeded)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }

                TempData["Error"] = result.Message;
                return RedirectToPage(new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while canceling the payment.";
                return RedirectToPage("index");
            }
        }
    }
}
