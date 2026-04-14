using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrahamSchoolAdminSystemWeb.Pages.admin
{
    [Authorize]
    [RequireRole(SD.Roles.ADMIN)]
    public class app_settingsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewsSelectionOptions _viewsSelectionOptions;

        [BindProperty]
        public AppSettingViewModel appsettings { get; set; }
        public List<SelectListItem> SessionList { get; set; } = new();
        public app_settingsModel(IUnitOfWork unitOfWork, IViewsSelectionOptions viewsSelectionOptions)
        {
            _unitOfWork = unitOfWork;
            _viewsSelectionOptions = viewsSelectionOptions;
        }
        public async Task OnGetAsync()
        {
            appsettings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
            await LoadSessionListAsync();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSessionListAsync();
                return Page();
            }
            var result = await _unitOfWork.UsersServices.CreateOrUpdateAppSettingsAsync(appsettings);
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToPage();
            }
            else
            {
                TempData["Error"] = result.Message;
                await LoadSessionListAsync();
                return Page();
            }
        }

        private async Task LoadSessionListAsync()
        {
            var sessions = await _viewsSelectionOptions.GetSessionsForDropdownAsync();
            SessionList = sessions.ToList();
        }
    }
}
