using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.employees
{
    [Authorize]
    [RequireRole(SD.Roles.ADMIN)]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public EmployeeViewModel EmployeeModel { get; set; }

        //public List<PositionDto> AvailablePositions { get; set; } = new();

        public IndexModel(IUnitOfWork unitOfWork, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAddEmployeeAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var result = await _unitOfWork.UsersServices.CreateEmployeeAsync(EmployeeModel);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "Employee",
                        entityId: result.Data?.ToString() ?? "Unknown",
                        message: $"Employee '{EmployeeModel.FirstName} {EmployeeModel.LastName}' created successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Email: {EmployeeModel.Email}, Phone: {EmployeeModel.Phone}, PositionId: {EmployeeModel.PositionId}"
                    );

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage();
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Employee Creation Error",
                    message: $"Error creating employee {EmployeeModel.FirstName} {EmployeeModel.LastName}",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["ErrorMessage"] = "Error creating employee";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostUpdateEmployeeAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var oldEmployee = await _unitOfWork.UsersServices.GetEmployeeByIdAsync((int)EmployeeModel.Id);
                var result = await _unitOfWork.UsersServices.UpdateEmployeeAsync(EmployeeModel);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "Employee",
                        entityId: EmployeeModel.Id.ToString(),
                        message: $"Employee '{EmployeeModel.FirstName} {EmployeeModel.LastName}' updated successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Email: {EmployeeModel.Email}, Phone: {EmployeeModel.Phone}, PositionId: {EmployeeModel.PositionId}"
                    );

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage();
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Employee Update Error",
                    message: $"Error updating employee {EmployeeModel.Id}",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["ErrorMessage"] = "Error updating employee";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(int employeeId)
        {
            try
            {
                var employee = await _unitOfWork.UsersServices.GetEmployeeByIdAsync(employeeId);
                var result = await _unitOfWork.UsersServices.DeleteEmployeeAsync(employeeId);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "Employee",
                        entityId: employeeId.ToString(),
                        message: "Employee deleted successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Deleted Employee: {employee?.FirstName} {employee?.LastName}"
                    );

                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Employee Deletion Error",
                    message: $"Error deleting employee {employeeId}",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["ErrorMessage"] = "Error deleting employee";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetEditEmployeeAsync(int id)
        {
            try
            {
                var employee = await _unitOfWork.UsersServices.GetEmployeeByIdAsync(id);
                if (employee == null)
                    return NotFound();

                return new JsonResult(new
                {
                    id = employee.Id,
                    firstName = employee.FirstName,
                    lastName = employee.LastName,
                    email = employee.Email,
                    phone = employee.Phone,
                    department = employee.Department,
                    positionId = employee.PositionId,
                    isActive = employee.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading employee {id}");
                return StatusCode(500, new { error = "Error loading employee" });
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedIp = HttpContext.Request.Headers["X-Forwarded-For"]
                    .ToString().Split(',').FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedIp))
                {
                    ipAddress = forwardedIp.Trim();
                }
            }
            return ipAddress ?? "Unknown";
        }
    }
}
