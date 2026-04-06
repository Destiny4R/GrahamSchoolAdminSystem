using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.fees_payment
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();

        public indexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the page.";
                return Page();
            }
        }
    }
}
