using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.payment_items
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OtherPayItemsViewModel viewModel { get; set; } 
        public indexModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Provide all fields";
                return Page();
            }
            var result = await _unitOfWork.OtherPaymentServices.CreateAndUpdateItemAsync(viewModel);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
            }
            else
                TempData["Error"] = result.Message;
            return Page();
        }
    }
}
