using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.feesmanager.fees_payment
{
    public class make_paymentModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();
        
        public RecordPaymentViewModel record { get; set; } = new();

        public RecordPaymeentSearchViewModel recordsearch { get; set; } = new();

        public make_paymentModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync().Result;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostSearchAsync(RecordPaymeentSearchViewModel recordsearch)
        {
            var result = await _unitOfWork.FeesPaymentServices.SearchFeesPaymentAsync(recordsearch);
            if(!result.Succecced)
            {
                TempData["Error"] =  result.Message;
                return Page();
            }
            TempData["Success"] = result.Message;
            record = result.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostMakePaymentAsync(RecordPaymentViewModel record)
        {
            if(record.evidenncefile == null || record.evidenncefile.Length == 0)
            {
                TempData["Error"] = "Please upload evidence of payment.";
                return Page();
            }
            else
            {
                // Define upload directory for payment evidence files
                //string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payment-evidence");

                //// Process and save the file
                //var fileUploadResult = FileUploadHandler.ProcessFile(record.evidenncefile, uploadDirectory);
                //if (!fileUploadResult.Success)
                //{
                //    TempData["Error"] = fileUploadResult.Message;
                //    return Page();
                //}

                //// Store the file path in the record for database storage
                //record.EvidenceFilePath = fileUploadResult.FileName;

                var result = await _unitOfWork.FeesPaymentServices.CreateFeesPaymentAsync(record);
                if(!result.Succeeded)
                {
                    TempData["Error"] =  result.Message;
                    return Page();
                }

                TempData["Success"] = result.Message;

                // Redirect to invoice page with the payment ID
                int paymentId = result.Data;
                return RedirectToPage("index");
            }
        }
    }
}
