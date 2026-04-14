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
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewsSelectionOptions _viewsSelectionOptions;

        public ViewSelections Selections { get; set; } = new();

        public indexModel(IUnitOfWork unitOfWork, IViewsSelectionOptions viewsSelectionOptions)
        {
            _unitOfWork = unitOfWork;
            _viewsSelectionOptions = viewsSelectionOptions;
        }

        public async Task OnGetAsync()
        {
            await LoadSelectionsAsync();
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
