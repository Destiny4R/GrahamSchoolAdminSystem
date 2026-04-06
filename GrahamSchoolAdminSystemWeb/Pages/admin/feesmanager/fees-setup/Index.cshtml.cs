using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.feesmanager.fees_setup
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public FeesSetupViewModel FeesSetupModel { get; set; }

        public ViewSelections Selections { get; set; } = new();

        public IndexModel(IUnitOfWork unitOfWork, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            Selections = _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync().Result;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

       
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid form data";
                return Page();
            }

            try
            {
                if (FeesSetupModel.Id > 0)
                {
                    var result = await _unitOfWork.FinanceServices.UpdateFeesSetupAsync(FeesSetupModel);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = result.Message;
                        return Page();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
                else
                {
                    var result = await _unitOfWork.FinanceServices.CreateFeesSetupAsync(FeesSetupModel);

                    if (result.Succeeded)
                    {
                        TempData["Success"] = result.Message;
                        return Page();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fees setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Fees Setup Creation Error",
                    message: "Error creating fees setup",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["Error"] = "An error occurred while processing your request.";
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
