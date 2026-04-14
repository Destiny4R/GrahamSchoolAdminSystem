using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.payment_items
{
    [Authorize]
    [RequirePermission(SD.Permissions.REPORT)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public PaymentItemViewModel ItemModel { get; set; }

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
                if (ItemModel.Id > 0)
                {
                    var result = await _unitOfWork.PaymentItemService.UpdatePaymentItemAsync(ItemModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Update",
                            entityType: "PaymentItem",
                            entityId: ItemModel.Id.ToString(),
                            message: $"Payment item '{ItemModel.Name}' updated successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Item Name: {ItemModel.Name}, Category: {ItemModel.CategoryId}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
                else
                {
                    var result = await _unitOfWork.PaymentItemService.CreatePaymentItemAsync(ItemModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Create",
                            entityType: "PaymentItem",
                            entityId: result.Data.ToString(),
                            message: $"Payment item '{ItemModel.Name}' created successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Item Name: {ItemModel.Name}, Category: {ItemModel.CategoryId}"
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
                _logger.LogError(ex, "Error processing payment item");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Payment Item Processing Error",
                    message: "Error processing payment item",
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
