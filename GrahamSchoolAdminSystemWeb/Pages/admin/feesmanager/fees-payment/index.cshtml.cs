using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.feesmanager.fees_payment
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ViewSelections Selections { get; set; } = new();

        public indexModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            Selections = _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync().Result;
        }
        public void OnGet()
        {
        }
    }
}
