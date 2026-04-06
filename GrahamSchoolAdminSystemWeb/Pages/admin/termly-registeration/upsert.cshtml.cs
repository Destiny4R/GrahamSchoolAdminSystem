using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.termly_registeration
{
    public class upsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();

        [BindProperty]
        public TermRegistrationViewModel termReg { get; set; } = new();
        public upsertModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync().Result;
        }
        public async Task<IActionResult> OnGet(int id)
        {
            if (id > 0)
            {
                termReg = _unitOfWork.TermRegistrationServices.GetStudentTermRegistrationByIdAsync(id).Result;
                if (termReg == null)
                {
                    TempData["Error"] = "Student not found, please check and try again";
                    return RedirectToPage("index");
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync(string termregnumber)
        {
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
                TempData["Error"] = "Provide select all areas";
                return Page();
            }

            if (termReg.Id > 0)
            {
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
                return Page();
            }
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
