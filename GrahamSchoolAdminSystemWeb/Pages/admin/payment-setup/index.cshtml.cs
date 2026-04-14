using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.payment_setup
{
    [Authorize]
    [RequirePermission(SD.Permissions.REPORT)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public PaymentSetupViewModel SetupModel { get; set; }

        public indexModel(IUnitOfWork unitOfWork, ILogger<indexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                if (SetupModel.Id > 0)
                {
                    var result = await _unitOfWork.PaymentSetupService.UpdatePaymentSetupAsync(SetupModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Update",
                            entityType: "PaymentSetup",
                            entityId: SetupModel.Id.ToString(),
                            message: $"Payment setup updated successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Item: {SetupModel.PaymentItemId}, Session: {SetupModel.SessionId}, Term: {SetupModel.Term}, Class: {SetupModel.ClassId}, Amount: {SetupModel.Amount}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
                else
                {
                    var result = await _unitOfWork.PaymentSetupService.CreatePaymentSetupAsync(SetupModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Create",
                            entityType: "PaymentSetup",
                            entityId: result.Data.ToString(),
                            message: $"Payment setup created successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Item: {SetupModel.PaymentItemId}, Session: {SetupModel.SessionId}, Term: {SetupModel.Term}, Class: {SetupModel.ClassId}, Amount: {SetupModel.Amount}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Payment Setup Processing Error",
                    message: "Error processing payment setup",
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
