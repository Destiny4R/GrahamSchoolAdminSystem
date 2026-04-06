using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.academicsession
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public SessionYearViewModel Model { get; set; }

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

            var result = await _unitOfWork.SystemActivities.CreateAcademicSessionAsync(Model);

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

            var result = await _unitOfWork.SystemActivities.UpdateAcademicSessionAsync(Model);

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
            var result = await _unitOfWork.SystemActivities.DeleteAcademicSessionAsync(id, "Deleted by admin");

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
