using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.termly_registeration
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class batch_registrationModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewsSelectionOptions _viewsSelectionOptions;

        public ViewSelections Selections { get; set; } = new();
        public SelectOptionsData Selectsearch { get; set; }

        public List<TermRegistration> TermReg { get; set; } = new List<TermRegistration>();
        public SelectOptionsDataForTermReg termRegBatch { get; set; }
        public batch_registrationModel(IUnitOfWork unitOfWork, IViewsSelectionOptions viewsSelectionOptions)
        {
            _unitOfWork = unitOfWork;
            _viewsSelectionOptions = viewsSelectionOptions;
        }

        public async Task OnGetAsync()
        {
            await LoadSelectionsAsync();
        }

        public async Task<IActionResult> OnPostSearchAsync(SelectOptionsData Selectsearch)
        {
            await LoadSelectionsAsync();
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
            await LoadSelectionsAsync();
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

        private async Task LoadSelectionsAsync()
        {
            Selections.SchoolClasses = await _viewsSelectionOptions.GetSchoolClassesForDropdownAsync();
            Selections.AcademicSession = await _viewsSelectionOptions.GetSessionsForDropdownAsync();
            Selections.SubClass = await _viewsSelectionOptions.GetSchoolSubclassesForDropdownAsync();
            Selections.Terms = Enum.GetValues<GetEnums.Term>()
                .Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
        }
    }
}
