using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.ptamanager.pta_fees_setup
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public PTAFeesSetupViewModel PTAFeesSetupModel { get; set; }

        public ViewSelections Selections { get; set; } = new();

        public IndexModel(IUnitOfWork unitOfWork, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            Selections = _unitOfWork.FinanceServices.GetPTAFeesSetupSelectionsAsync().Result;
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
                if (PTAFeesSetupModel.Id > 0)
                {
                    var result = await _unitOfWork.FinanceServices.UpdatePTAFeesSetupAsync(PTAFeesSetupModel);
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
                    var result = await _unitOfWork.FinanceServices.CreatePTAFeesSetupAsync(PTAFeesSetupModel);

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
                _logger.LogError(ex, "Error creating PTA fees setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "PTA Fees Setup Creation Error",
                    message: "Error creating PTA fees setup",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["Error"] = "An error occurred while processing your request.";
                return Page();
            }
        }

        

        public async Task<IActionResult> OnGetGetPTAFeesSetupByIdAsync(int id)
        {
            try
            {
                var ptaFeesSetup = await _unitOfWork.FinanceServices.GetPTAFeesSetupByIdAsync(id);
                if (ptaFeesSetup == null)
                    return NotFound();

                return new JsonResult(ptaFeesSetup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PTA fees setup");
                return new JsonResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnDeleteDeletePTAFeesSetupAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.FinanceServices.DeletePTAFeesSetupAsync(id);
                return new JsonResult(new { succeeded = result.Succeeded, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PTA fees setup");
                return new JsonResult(new { succeeded = false, message = ex.Message }) { StatusCode = 500 };
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
