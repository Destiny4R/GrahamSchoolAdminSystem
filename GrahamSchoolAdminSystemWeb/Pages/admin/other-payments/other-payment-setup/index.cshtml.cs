using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.other_payment_setup
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public OtherFeesSetupViewModel OtherFeesSetupModel { get; set; }

        public ViewSelections Selections { get; set; } = new();

        public indexModel(IUnitOfWork unitOfWork, ILogger<indexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                _logger.LogError(ex, "Error loading page");
                TempData["Error"] = "An error occurred while loading the page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid form data";
                Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                return Page();
            }

            try
            {
                if (OtherFeesSetupModel.Id > 0)
                {
                    var result = await _unitOfWork.FinanceServices.UpdateOtherFeesSetupAsync(OtherFeesSetupModel);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = result.Message;
                        Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                        return Page();
                    }
                    TempData["Error"] = result.Message;
                    Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                    return Page();
                }
                else
                {
                    var result = await _unitOfWork.FinanceServices.CreateOtherFeesSetupAsync(OtherFeesSetupModel);

                    if (result.Succeeded)
                    {
                        TempData["Success"] = result.Message;
                        Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                        return Page();
                    }
                    TempData["Error"] = result.Message;
                    Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating other fees setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Other Fees Setup Creation Error",
                    message: "Error creating other fees setup",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["Error"] = "An error occurred while processing your request.";
                Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetGetOtherFeesSetupByIdAsync(int id)
        {
            try
            {
                var otherFeesSetup = await _unitOfWork.FinanceServices.GetOtherFeesSetupByIdAsync(id);
                if (otherFeesSetup == null)
                    return NotFound();

                return new JsonResult(otherFeesSetup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving other fees setup");
                return new JsonResult(new { error = ex.Message }) { StatusCode = 500 };
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
