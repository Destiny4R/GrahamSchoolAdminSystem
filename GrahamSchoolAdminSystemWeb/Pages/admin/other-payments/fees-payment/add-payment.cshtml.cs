using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.fees_payment
{
    public class add_paymentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();

        public RecordPaymentViewModel Record { get; set; } = new();

        public RecordPaymeentSearchViewModel RecordSearch { get; set; } = new();

        public add_paymentModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync().Result;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSearchAsync(RecordPaymeentSearchViewModel recordSearch)
        {
            try
            {
                var result = await _unitOfWork.OtherPaymentServices.SearchOtherPaymentAsync(recordSearch);
                if (!result.Succeeded)
                {
                    TempData["Error"] = result.Message;
                    return Page();
                }

                TempData["Success"] = result.Message;
                Record = result.Data;
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error searching payment records.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMakePaymentAsync(RecordPaymentViewModel record)
        {
            try
            {
                if (record.evidenncefile == null || record.evidenncefile.Length == 0)
                {
                    TempData["Error"] = "Please upload evidence of payment.";
                    return Page();
                }

                // Create payment record
                var result = await _unitOfWork.OtherPaymentServices.CreateOtherPaymentAsync(record);
                if (!result.Succeeded)
                {
                    TempData["Error"] = result.Message;
                    return Page();
                }

                TempData["Success"] = "Payment recorded successfully.";
                return RedirectToPage("index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error recording payment.";
                return Page();
            }
        }
    }
}
