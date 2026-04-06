using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.termly_registeration
{
    public class batch_registrationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();
        public SelectOptionsData Selectsearch { get; set; }

        public List<TermRegistration> TermReg { get; set; } = new List<TermRegistration>();
        public SelectOptionsDataForTermReg termRegBatch { get; set; }
        public batch_registrationModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync().Result;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostSearchAsync(SelectOptionsData Selectsearch)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select all required fields.";
                return Page();
            }
            TermReg = await _unitOfWork.TermRegistrationServices.GetOldTermRegRecordsAsync(Selectsearch);
            if (!TermReg.Any())
            {
                TempData["Error"] = "No record found";
                return Page();
            }
            TempData["Success"] = "Records found successfully";
            return Page();
        }
        public async Task<IActionResult> OnPostRegisterAsync(SelectOptionsDataForTermReg termRegBatch)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please select all required fields.";
                return Page();
            }
            int successCount = 0, failureCount = 0;
            foreach (var studentId in termRegBatch.studentsid)
            {
                var registrationData = new TermRegistrationViewModel
                {
                    StudentId = studentId,
                    Term = (GetEnums.Term)termRegBatch.data.term,
                    SessionId = termRegBatch.data.sessionid,
                    SchoolClassId = termRegBatch.data.schoolclass,
                    SchoolSubclassId = termRegBatch.data.subclass
                };

                var existingRecord = await _unitOfWork.TermRegistrationServices.CreateStudentTermRegistrationAsync(registrationData);
                if (existingRecord.Succeeded)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                }
            }
            TempData["Success"] = $"Students registered successfully. Success: {successCount}, Failure: {failureCount}";
            return RedirectToPage("index");
        }
    }
}
