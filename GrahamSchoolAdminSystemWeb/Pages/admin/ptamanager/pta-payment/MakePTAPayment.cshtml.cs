using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.ptamanager.pta_payment
{
    public class MakePTAPaymentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();

        public RecordPTAPaymentViewModel Record { get; set; } = new();

        public RecordPTAPaymentSearchViewModel RecordSearch { get; set; } = new();

        public MakePTAPaymentModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetPTAPaymentSelectionsAsync().Result;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSearchAsync(RecordPTAPaymentSearchViewModel recordSearch)
        {
            try
            {
                var result = await _unitOfWork.PTAPaymentServices.SearchPTAPaymentAsync(recordSearch);
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
                TempData["Error"] = "An error occurred while searching for payment records.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostMakePaymentAsync(RecordPTAPaymentViewModel record)
        {
            try
            {
                if (record.evidencefile == null || record.evidencefile.Length == 0)
                {
                    TempData["Error"] = "Please upload evidence of payment.";
                    return Page();
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please fill all required fields correctly.";
                    return Page();
                }

                var result = await _unitOfWork.PTAPaymentServices.CreatePTAPaymentAsync(record);

                if (result.Succeeded)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }

                TempData["Error"] = result.Message;
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred while recording the payment.";
                return Page();
            }
        }
    }
}
