using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.sub_class
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public SchoolSubClassViewModel Model { get; set; }

        public indexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }

        // CREATE
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return Page();
            }

            var result = await _unitOfWork.SystemActivities.CreateSchoolSubClassAsync(Model);

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToPage();
            }

            TempData["Error"] = result.Message;
            return RedirectToPage();
        }

        // UPDATE
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return RedirectToPage();
            }

            var result = await _unitOfWork.SystemActivities.UpdateSchoolSubClassAsync(Model);

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToPage();
        }

        // DELETE
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _unitOfWork.SystemActivities.DeleteSchoolSunClassAsync(id, "Deleted by admin");

            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToPage();
        }
    }
}

