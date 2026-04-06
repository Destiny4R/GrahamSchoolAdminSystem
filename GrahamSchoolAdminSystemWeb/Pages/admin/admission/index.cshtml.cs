using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.admission
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public StudentViewModel StudentModel { get; set; }

        public indexModel(IUnitOfWork unitOfWork, ILogger<indexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageResult> OnGetAsync()
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
                if (StudentModel.Id > 0)
                {
                    // Update existing school class
                    var result = await _unitOfWork.StudentServices.UpdateStudentAsync(StudentModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Update",
                            entityType: "StudentTable",
                            entityId: StudentModel.Id.ToString(),
                            message: $"Student record '{StudentModel.Firstname} {StudentModel.Surname}' updated successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Student Name: {StudentModel.Firstname} {StudentModel.Surname}"
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
                    var result = await _unitOfWork.StudentServices.CreateStudentAsync(StudentModel);

                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Create",
                            entityType: "StudentTable",
                            entityId: result.Data.ToString(),
                            message: $"Student Record '{StudentModel.Firstname} {StudentModel.Surname}' created successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Student Name: {StudentModel.Firstname} {StudentModel.Surname}"
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
                _logger.LogError(ex, "Error processing Student admission");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student admission Processing Error",
                    message: "Error processing student admission",
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
