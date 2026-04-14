using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.schoolclass
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public SchoolClassViewModel SchoolClassModel { get; set; }

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
                if (SchoolClassModel.Id > 0)
                {
                    // Update existing school class
                    var result = await _unitOfWork.SchoolClassServices.UpdateSchoolClassAsync(SchoolClassModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Update",
                            entityType: "SchoolClass",
                            entityId: SchoolClassModel.Id.ToString(),
                            message: $"School class '{SchoolClassModel.Name}' updated successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Class Name: {SchoolClassModel.Name}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
                else
                {
                    // Create new school class
                    var result = await _unitOfWork.SchoolClassServices.CreateSchoolClassAsync(SchoolClassModel);

                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Create",
                            entityType: "SchoolClass",
                            entityId: result.Data.ToString(),
                            message: $"School class '{SchoolClassModel.Name}' created successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Class Name: {SchoolClassModel.Name}"
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
                _logger.LogError(ex, "Error processing school class");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "School Class Processing Error",
                    message: "Error processing school class",
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
