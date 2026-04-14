using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.termly_registeration
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class upsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewsSelectionOptions _viewsSelectionOptions;

        public ViewSelections Selections { get; set; } = new();

        [BindProperty]
        public TermRegistrationViewModel termReg { get; set; } = new();

        public bool HasPayment { get; set; }

        public upsertModel(IUnitOfWork unitOfWork, IViewsSelectionOptions viewsSelectionOptions)
        {
            _unitOfWork = unitOfWork;
            _viewsSelectionOptions = viewsSelectionOptions;
        }
        public async Task<IActionResult> OnGet(int id)
        {
            await LoadSelectionsAsync();
            if (id > 0)
            {
                termReg = _unitOfWork.TermRegistrationServices.GetStudentTermRegistrationByIdAsync(id).Result;
                if (termReg == null)
                {
                    TempData["Error"] = "Student not found, please check and try again";
                    return RedirectToPage("index");
                }

                // Check if payments exist for this registration
                HasPayment = await _unitOfWork.TermRegistrationServices.HasPaymentForTermRegistrationAsync(id);
                if (HasPayment)
                {
                    TempData["Error"] = "This registration cannot be edited because payment records exist for it.";
                    return RedirectToPage("index");
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync(string termregnumber)
        {
            await LoadSelectionsAsync();
            if (string.IsNullOrEmpty(termregnumber))
            {
                TempData["Error"] = "Provide student registration number before proceeding";
                return Page();
            }
            var result = _unitOfWork.StudentServices.GetStudentByIdAsync(termregnumber).Result;
            if (result == null)
            {
                TempData["Error"] = "Student not found, please check and try again";
                return Page();
            }
            termReg = new TermRegistrationViewModel
            {
                StudentId = result.Id,
                //Id = result.Id,
                StudentRegNumber = result.ApplicationUser.UserName,
                StudentName = $"{result.Surname} {result.Firstname} {result.Othername}"
            };
            TempData["Success"] = "Student found successfully";
            return Page();
        }

        //Register
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectionsAsync();
                TempData["Error"] = "Provide select all areas";
                return Page();
            }

            if (termReg.Id > 0)
            {
                // Block update if payments exist
                var hasPayment = await _unitOfWork.TermRegistrationServices.HasPaymentForTermRegistrationAsync((int)termReg.Id);
                if (hasPayment)
                {
                    TempData["Error"] = "This registration cannot be edited because payment records exist for it.";
                    return RedirectToPage("index");
                }

                var result = _unitOfWork.TermRegistrationServices.UpdateStudentTermRegistrationAsync(termReg).Result;
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "TermRegistration",
                        entityId: termReg.Id.ToString(),
                        message: $"Term registration updated for student ID {termReg.StudentId}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class: {termReg.SchoolClassId}, Term: {termReg.Term}"
                    );

                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                TempData["Error"] = result.Message;
                await LoadSelectionsAsync();
                return Page();
            }
            else
            {
                var result = _unitOfWork.TermRegistrationServices.CreateStudentTermRegistrationAsync(termReg).Result;
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "TermRegistration",
                        entityId: result.Data.ToString(),
                        message: $"Term registration created for student ID {termReg.StudentRegNumber}: {DateTime.UtcNow}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class: {termReg.SchoolClassId}, Session: {termReg.SessionId}, Term: {termReg.Term}"
                    );
                    TempData["Success"] = result.Message;
                    return RedirectToPage("index");
                }
                TempData["Error"] = result.Message;
                await LoadSelectionsAsync();
                return Page();
            }
        }

        private async Task LoadSelectionsAsync()
        {
            Selections.SchoolClasses = await _viewsSelectionOptions.GetSchoolClassesForDropdownAsync();
            Selections.AcademicSession = await _viewsSelectionOptions.GetSessionsForDropdownAsync();
            Selections.SubClass = await _viewsSelectionOptions.GetSchoolSubclassesForDropdownAsync();
            Selections.Terms = Enum.GetValues<GetEnums.Term>()
                .Select(t => new SelectListItem { Value = ((int)t).ToString(), Text = t.ToString() });
        }

        private string GetClientIpAddress()
        {
            try
            {
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                    return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
                return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
