using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemWeb.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1")]
    public class v1Controller : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<v1Controller> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IViewsSelectionOptions _viewsSelection;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<PaymentNotificationHub> _hubContext;

        public v1Controller(
            IUnitOfWork unitOfWork, 
            ILogger<v1Controller> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IPermissionService permissionService,
            IViewsSelectionOptions viewsSelection,
            IWebHostEnvironment env,
            IHubContext<PaymentNotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _permissionService = permissionService;
            _viewsSelection = viewsSelection;
            _env = env;
            _hubContext = hubContext;
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


        #region School Classes

        
        [HttpPost("schoolclasses/create")]
        public async Task<IActionResult> CreateSchoolClass([FromBody] SchoolClassViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.SchoolClassServices.CreateSchoolClassAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "SchoolClass",
                        entityId: result.Data.ToString(),
                        message: $"School class '{model.Name}' created successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class Name: {model.Name}"
                    );

                    return Json(new { success = true, message = result.Message, id = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating school class");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "School Class Creation Error",
                    message: "Error creating school class",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error creating school class" });
            }
        }

        /// <summary>
        /// GET: api/v1/schoolclasses/5 - Get school class by ID for editing
        /// </summary>
        [HttpGet("schoolclasses/{id}")]
        public async Task<IActionResult> GetSchoolClass(int id)
        {
            try
            {
                var schoolClass = await _unitOfWork.SchoolClassServices.GetSchoolClassByIdAsync(id);
                if (schoolClass == null)
                    return Json(new { error = "School class not found" });

                return Json(new { success = true, data = schoolClass });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading school class");
                return Json(new { error = "Error loading data" });
            }
        }

        /// <summary>
        /// PUT: api/v1/schoolclasses/update - Update school class
        /// </summary>
        [HttpPut("schoolclasses/update")]
        public async Task<IActionResult> UpdateSchoolClass([FromBody] SchoolClassViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.SchoolClassServices.UpdateSchoolClassAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "SchoolClass",
                        entityId: model.Id.ToString(),
                        message: $"School class '{model.Name}' updated successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class Name: {model.Name}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating school class");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "School Class Update Error",
                    message: "Error updating school class",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error updating school class" });
            }
        }

        /// <summary>
        /// DELETE: api/v1/schoolclasses/5 - Delete school class (with SweetAlert2 confirmation)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteSchoolClass(int id)
        {
            try
            {
                var result = await _unitOfWork.SchoolClassServices.DeleteSchoolClassAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "SchoolClass",
                        entityId: id.ToString(),
                        message: "School class deleted successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Deleted school class with ID: {id}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting school class");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "School Class Delete Error",
                    message: "Error deleting school class",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting school class" });
            }
        }

        #endregion

        #region Students

        [HttpPost("students/create")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.StudentServices.CreateStudentAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "Student",
                        entityId: result.Data.ToString(),
                        message: $"Student '{model.FullName}' created successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Email: {model.Email}, Gender: {model.GenderDisplay}"
                    );

                    return Json(new { success = true, message = result.Message, id = result.Data });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Creation Error",
                    message: "Error creating student",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error creating student" });
            }
        }

        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            try
            {
                var student = await _unitOfWork.StudentServices.GetStudentByIdAsync(id);
                if (student == null)
                    return Json(new { error = "Student not found" });

                return Json(new { success = true, data = student });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPut("students/update")]
        public async Task<IActionResult> UpdateStudent([FromBody] StudentViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.StudentServices.UpdateStudentAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "Student",
                        entityId: model.Id.ToString(),
                        message: $"Student '{model.FullName}' updated successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Email: {model.Email}, Gender: {model.GenderDisplay}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Update Error",
                    message: "Error updating student",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error updating student" });
            }
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var result = await _unitOfWork.StudentServices.DeleteStudentAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "Student",
                        entityId: id.ToString(),
                        message: "Student deleted successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Deleted student with ID: {id}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Delete Error",
                    message: "Error deleting student",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting student" });
            }
        }

       
        [HttpPost("students/{id}/activate")]
        public async Task<IActionResult> ActivateStudent(int id)
        {
            try
            {
                var result = await _unitOfWork.StudentServices.ToggleStudentActivationAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Activate",
                        entityType: "Student",
                        entityId: id.ToString(),
                        message: "Student account activated successfully",
                        ipAddress: GetClientIpAddress()
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating student");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Activation Error",
                    message: "Error activating student",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error activating student" });
            }
        }

        #endregion

        #region Authentication

        /// <summary>
        /// POST: api/v1/auth/logout - API endpoint for logout
        /// </summary>
        [HttpPost("auth/logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            var ipAddress = GetClientIpAddress();
            var userId = user?.Id;
            var userName = user?.UserName ?? User.Identity?.Name ?? "Unknown";

            try
            {
                // Log the logout action before signing out
                if (user != null)
                {
                    await _unitOfWork.LogService.LogAuthenticationAsync(
                        userId: userId,
                        userName: userName,
                        action: "Logout",
                        ipAddress: ipAddress,
                        success: true,
                        message: $"User {userName} logged out via API",
                        details: $"API logout from IP: {ipAddress}"
                    );

                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: userId,
                        userName: userName,
                        action: "Logout",
                        entityType: "Authentication",
                        entityId: userId,
                        message: $"User {userName} signed out via API",
                        ipAddress: ipAddress,
                        details: "API-based logout"
                    );

                    _logger.LogInformation($"User {userName} logged out via API from IP: {ipAddress}");
                }

                // Sign out the user
                await _signInManager.SignOutAsync();

                return Json(new
                {
                    success = true,
                    message = "You have been logged out successfully.",
                    redirectUrl = "/account/login"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during API logout for user: {userName}");

                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "API Logout Error",
                    message: $"An error occurred during API logout for user {userName}",
                    details: ex.Message,
                    userId: userId,
                    ipAddress: ipAddress
                );

                // Still try to sign out even if logging fails
                await _signInManager.SignOutAsync();

                return Json(new
                {
                    success = false,
                    message = "An error occurred during logout, but you have been signed out.",
                    redirectUrl = "/account/login"
                });
            }
        }

        /// <summary>
        /// GET: api/v1/auth/session-status - Check if user session is valid
        /// </summary>
        [AllowAnonymous]
        [HttpGet("auth/session-status")]
        public IActionResult GetSessionStatus()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            var userName = User?.Identity?.Name;

            return Json(new
            {
                isAuthenticated = isAuthenticated,
                userName = userName,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// GET: api/v1/auth/permissions - Get current user's permissions
        /// </summary>
        [HttpGet("auth/permissions")]
        [Authorize]
        public async Task<IActionResult> GetUserPermissions()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                return Json(new
                {
                    success = true,
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    roles = roles,
                    permissions = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return Json(new { success = false, message = "Error retrieving permissions" });
            }
        }

        #endregion

        #region Term Registration

        [HttpGet("termregistration/{id}")]
        public async Task<IActionResult> GetTermRegistrationById(int id)
        {
            try
            {
                var result = await _unitOfWork.TermRegistrationServices.GetStudentTermRegistrationByIdAsync(id);

                if (result == null)
                    return Json(new { success = false, message = "Term registration not found" });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting term registration");
                return Json(new { success = false, message = "Error retrieving term registration" });
            }
        }


       
        [HttpDelete("termregistration/{id}")]
        public async Task<IActionResult> DeleteTermRegistration(int id)
        {
            try
            {
                var result = await _unitOfWork.TermRegistrationServices.DeleteStudentTermRegistrationAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "TermRegistration",
                        entityId: id.ToString(),
                        message: "Term registration deleted successfully",
                        ipAddress: GetClientIpAddress()
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting term registration");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Term Registration Delete Error",
                    message: "Error deleting term registration",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting term registration" });
            }
        }

        [HttpPost("termregistration/deletemultiple")]
        public async Task<IActionResult> DeleteMultipleTermRegistrations([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return Json(new { success = false, message = "No registrations selected" });

                var result = await _unitOfWork.TermRegistrationServices.DeleteStudentsTermRegistrationAsync(ids);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "DeleteMultiple",
                        entityType: "TermRegistration",
                        entityId: string.Join(",", ids),
                        message: $"Bulk deleted {ids.Count} term registrations",
                        ipAddress: GetClientIpAddress()
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting term registrations");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Term Registration Bulk Delete Error",
                    message: "Error bulk deleting term registrations",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting registrations" });
            }
        }

        [HttpPost("termregistration/batch-excel")]
        public async Task<IActionResult> BatchRegisterFromExcel(
            [FromForm] IFormFile excelFile,
            [FromForm] int sessionId,
            [FromForm] int term,
            [FromForm] int schoolClassId,
            [FromForm] int schoolSubclassId)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                    return Json(new { success = false, message = "Please upload an Excel file" });

                var extension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".xls")
                    return Json(new { success = false, message = "Only .xlsx or .xls files are allowed" });

                var regNumbers = new List<string>();
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    stream.Position = 0;

                    using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                    var worksheet = workbook.Worksheets.First();
                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                    for (int row = 1; row <= lastRow; row++)
                    {
                        var cellValue = worksheet.Cell(row, 1).GetString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            regNumbers.Add(cellValue);
                        }
                    }
                }

                if (!regNumbers.Any())
                    return Json(new { success = false, message = "No registration numbers found in the Excel file" });

                int successCount = 0;
                int failureCount = 0;
                var errors = new List<string>();

                foreach (var regNumber in regNumbers)
                {
                    var student = await _unitOfWork.StudentServices.GetStudentByIdAsync(regNumber);
                    if (student == null)
                    {
                        failureCount++;
                        errors.Add($"{regNumber} — Student not found");
                        continue;
                    }

                    var registrationData = new TermRegistrationViewModel
                    {
                        StudentId = student.Id,
                        Term = (GetEnums.Term)term,
                        SessionId = sessionId,
                        SchoolClassId = schoolClassId,
                        SchoolSubclassId = schoolSubclassId
                    };

                    var result = await _unitOfWork.TermRegistrationServices.CreateStudentTermRegistrationAsync(registrationData);
                    if (result.Succeeded)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                        errors.Add($"{regNumber} — {result.Message}");
                    }
                }

                await _unitOfWork.LogService.LogUserActionAsync(
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    userName: User.Identity?.Name,
                    action: "BatchRegisterExcel",
                    entityType: "TermRegistration",
                    entityId: "batch",
                    message: $"Batch Excel registration: {successCount} success, {failureCount} failed out of {regNumbers.Count} total",
                    ipAddress: GetClientIpAddress()
                );

                return Json(new
                {
                    success = true,
                    message = $"Batch registration completed. {successCount} registered successfully, {failureCount} failed out of {regNumbers.Count} total.",
                    successCount,
                    failureCount,
                    total = regNumbers.Count,
                    errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch Excel registration");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Batch Excel Registration Error",
                    message: "Error during batch Excel registration",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "An error occurred during batch registration" });
            }
        }

        #endregion

        #region Payment Categories

        [HttpPost("paymentcategories/create")]
        public async Task<IActionResult> CreatePaymentCategory([FromBody] PaymentCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentCategoryService.CreatePaymentCategoryAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "PaymentCategory",
                        entityId: result.Data.ToString(),
                        message: $"Payment category '{model.Name}' created",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message, id = result.Data });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment category");
                return Json(new { success = false, message = "Error creating payment category" });
            }
        }

        [HttpGet("paymentcategories/{id}")]
        public async Task<IActionResult> GetPaymentCategory(int id)
        {
            try
            {
                var category = await _unitOfWork.PaymentCategoryService.GetPaymentCategoryByIdAsync(id);
                if (category == null)
                    return Json(new { success = false, message = "Payment category not found" });
                return Json(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment category");
                return Json(new { success = false, message = "Error loading data" });
            }
        }

        [HttpGet("paymentcategories/active")]
        public async Task<IActionResult> GetActivePaymentCategories()
        {
            try
            {
                var categories = await _unitOfWork.PaymentCategoryService.GetActiveCategoriesAsync();
                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active payment categories");
                return Json(new { success = false, message = "Error loading data" });
            }
        }

        [HttpPut("paymentcategories/update")]
        public async Task<IActionResult> UpdatePaymentCategory([FromBody] PaymentCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentCategoryService.UpdatePaymentCategoryAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "PaymentCategory",
                        entityId: model.Id.ToString(),
                        message: $"Payment category '{model.Name}' updated",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment category");
                return Json(new { success = false, message = "Error updating payment category" });
            }
        }

        [HttpDelete("paymentcategories/{id}")]
        public async Task<IActionResult> DeletePaymentCategory(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentCategoryService.DeletePaymentCategoryAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "PaymentCategory",
                        entityId: id.ToString(),
                        message: "Payment category deleted",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment category");
                return Json(new { success = false, message = "Error deleting payment category" });
            }
        }

        [HttpPost("paymentcategories/{id}/toggle")]
        public async Task<IActionResult> TogglePaymentCategory(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentCategoryService.TogglePaymentCategoryAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "ToggleActivation",
                        entityType: "PaymentCategory",
                        entityId: id.ToString(),
                        message: $"Payment category activation toggled: {result.Message}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment category ID: {id}"
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling payment category");
                return Json(new { success = false, message = "Error toggling payment category" });
            }
        }

        #endregion

        #region Payment Items

        [HttpPost("paymentitems/create")]
        public async Task<IActionResult> CreatePaymentItem([FromBody] PaymentItemViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentItemService.CreatePaymentItemAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "PaymentItem",
                        entityId: result.Data.ToString(),
                        message: $"Payment item '{model.Name}' created",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message, id = result.Data });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment item");
                return Json(new { success = false, message = "Error creating payment item" });
            }
        }

        [HttpGet("paymentitems/{id}")]
        public async Task<IActionResult> GetPaymentItem(int id)
        {
            try
            {
                var item = await _unitOfWork.PaymentItemService.GetPaymentItemByIdAsync(id);
                if (item == null)
                    return Json(new { success = false, message = "Payment item not found" });
                return Json(new { success = true, data = item });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment item");
                return Json(new { success = false, message = "Error loading data" });
            }
        }

        [HttpGet("paymentitems/active")]
        public async Task<IActionResult> GetActivePaymentItems([FromQuery] int? categoryId = null)
        {
            try
            {
                var items = await _unitOfWork.PaymentItemService.GetActiveItemsAsync(categoryId);
                return Json(new { success = true, data = items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active payment items");
                return Json(new { success = false, message = "Error loading data" });
            }
        }

        [HttpPut("paymentitems/update")]
        public async Task<IActionResult> UpdatePaymentItem([FromBody] PaymentItemViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentItemService.UpdatePaymentItemAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "PaymentItem",
                        entityId: model.Id.ToString(),
                        message: $"Payment item '{model.Name}' updated",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment item");
                return Json(new { success = false, message = "Error updating payment item" });
            }
        }

        [HttpDelete("paymentitems/{id}")]
        public async Task<IActionResult> DeletePaymentItem(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentItemService.DeletePaymentItemAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "PaymentItem",
                        entityId: id.ToString(),
                        message: "Payment item deleted",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment item");
                return Json(new { success = false, message = "Error deleting payment item" });
            }
        }

        [HttpPost("paymentitems/{id}/toggle")]
        public async Task<IActionResult> TogglePaymentItem(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentItemService.TogglePaymentItemAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "ToggleActivation",
                        entityType: "PaymentItem",
                        entityId: id.ToString(),
                        message: $"Payment item activation toggled: {result.Message}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment item ID: {id}"
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling payment item");
                return Json(new { success = false, message = "Error toggling payment item" });
            }
        }

        #endregion

        #region Payment Setup

        [HttpPost("paymentsetups/create")]
        public async Task<IActionResult> CreatePaymentSetup([FromBody] PaymentSetupViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentSetupService.CreatePaymentSetupAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "PaymentSetup",
                        entityId: result.Data.ToString(),
                        message: $"Payment setup created for item {model.PaymentItemId}",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message, id = result.Data });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment setup");
                return Json(new { success = false, message = "Error creating payment setup" });
            }
        }

        [HttpGet("paymentsetups/{id}")]
        public async Task<IActionResult> GetPaymentSetup(int id)
        {
            try
            {
                var setup = await _unitOfWork.PaymentSetupService.GetPaymentSetupByIdAsync(id);
                if (setup == null)
                    return Json(new { success = false, message = "Payment setup not found" });
                return Json(new { success = true, data = setup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment setup");
                return Json(new { success = false, message = "Error loading data" });
            }
        }

        [HttpPut("paymentsetups/update")]
        public async Task<IActionResult> UpdatePaymentSetup([FromBody] PaymentSetupViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.PaymentSetupService.UpdatePaymentSetupAsync(model);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "PaymentSetup",
                        entityId: model.Id.ToString(),
                        message: $"Payment setup updated",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment setup");
                return Json(new { success = false, message = "Error updating payment setup" });
            }
        }

        [HttpDelete("paymentsetups/{id}")]
        public async Task<IActionResult> DeletePaymentSetup(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentSetupService.DeletePaymentSetupAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "PaymentSetup",
                        entityId: id.ToString(),
                        message: "Payment setup deleted",
                        ipAddress: GetClientIpAddress()
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment setup");
                return Json(new { success = false, message = "Error deleting payment setup" });
            }
        }

        [HttpPost("paymentsetups/{id}/toggle")]
        public async Task<IActionResult> TogglePaymentSetup(int id)
        {
            try
            {
                var result = await _unitOfWork.PaymentSetupService.TogglePaymentSetupAsync(id);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "ToggleActivation",
                        entityType: "PaymentSetup",
                        entityId: id.ToString(),
                        message: $"Payment setup activation toggled: {result.Message}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment setup ID: {id}"
                    );
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling payment setup");
                return Json(new { success = false, message = "Error toggling payment setup" });
            }
        }

        #endregion

        #region Student Payments

        [HttpGet("studentpayments/payable-items/{termRegId}")]
        public async Task<IActionResult> GetPayableItems(int termRegId)
        {
            try
            {
                var result = await _unitOfWork.StudentPaymentService.GetPayableItemsAsync(termRegId);
                if (result == null)
                    return Json(new { success = false, message = "Term registration not found" });
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payable items");
                return Json(new { success = false, message = "Error loading payable items" });
            }
        }

        [HttpPost("studentpayments/create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateStudentPayment()
        {
            try
            {
                if (!int.TryParse(Request.Form["termRegistrationId"].FirstOrDefault(), out int termRegistrationId))
                    return Json(new { success = false, message = "Invalid term registration ID" });

                var narration = Request.Form["narration"].FirstOrDefault();
                var itemsJson = Request.Form["items"].FirstOrDefault();
                if (string.IsNullOrEmpty(itemsJson))
                    return Json(new { success = false, message = "No payment items provided" });

                var items = JsonSerializer.Deserialize<List<StudentPaymentItemVM>>(itemsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var model = new CreatePaymentViewModel
                {
                    TermRegistrationId = termRegistrationId,
                    Narration = narration,
                    Items = items ?? new()
                };

                // Handle evidence file upload
                string? evidenceFilePath = null;
                var evidenceFile = Request.Form.Files.GetFile("evidence");
                if (evidenceFile != null)
                {
                    var validation = FileUploadHandler.ValidateFile(evidenceFile);
                    if (!validation.IsValid)
                        return Json(new { success = false, message = validation.ErrorMessage });

                    var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "payment-evidence");
                    var processResult = FileUploadHandler.ProcessFile(evidenceFile, uploadPath);
                    if (!processResult.Success)
                        return Json(new { success = false, message = processResult.Message });

                    evidenceFilePath = $"/uploads/payment-evidence/{processResult.FileName}";
                }

                var result = await _unitOfWork.StudentPaymentService.CreatePaymentAsync(model, evidenceFilePath);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "StudentPayment",
                        entityId: result.Data.ToString(),
                        message: $"Student payment created for term registration {model.TermRegistrationId}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Items: {model.Items.Count}, Total: {model.Items.Sum(i => i.AmountPaid)}"
                    );

                    // Broadcast real-time notification to Approvers group
                    await _hubContext.Clients.Group("Approvers").SendAsync("NewPaymentReceived", new
                    {
                        paymentId = result.Data,
                        amount = model.Items.Sum(i => i.AmountPaid),
                        createdBy = User.Identity?.Name ?? "Unknown"
                    });

                    return Json(new { success = true, message = result.Message, id = result.Data });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student payment");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Payment Creation Error",
                    message: "Error creating student payment",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );
                return Json(new { success = false, message = "Error creating student payment" });
            }
        }

        [HttpPost("studentpayments/lookup")]
        public async Task<IActionResult> LookupPayableItems([FromBody] PaymentLookupViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.StudentPaymentService.LookupPayableItemsAsync(
                    model.AdmissionNo, model.ClassId, model.CategoryId);

                await _unitOfWork.LogService.LogUserActionAsync(
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    userName: User.Identity?.Name,
                    action: "LookupPayableItems",
                    entityType: "StudentPayment",
                    entityId: model.AdmissionNo ?? "N/A",
                    message: $"Looked up payable items for admission no '{model.AdmissionNo}'",
                    ipAddress: GetClientIpAddress(),
                    details: $"AdmissionNo: {model.AdmissionNo}, ClassId: {model.ClassId}, CategoryId: {model.CategoryId}, Found: {result.Succeeded}"
                );

                return Json(new { success = result.Succeeded, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up payable items");
                return Json(new { success = false, message = "Error looking up payable items" });
            }
        }

        [HttpGet("studentpayments/receipt/{id}")]
        public async Task<IActionResult> GetPaymentReceipt(int id)
        {
            try
            {
                var receipt = await _unitOfWork.StudentPaymentService.GetReceiptAsync(id);
                if (receipt == null)
                    return Json(new { success = false, message = "Payment not found" });
                return Json(new { success = true, data = receipt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment receipt");
                return Json(new { success = false, message = "Error loading receipt" });
            }
        }

        [HttpPost("studentpayments/{id}/update-state")]
        public async Task<IActionResult> UpdatePaymentState(int id, [FromBody] UpdatePaymentStateRequest request)
        {
            try
            {
                // Only users with REPORT permission can approve/reject/cancel payments
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !await _permissionService.UserHasPermissionAsync(userId, SD.Permissions.REPORT))
                    return Json(new { success = false, message = "You do not have permission to perform this action" });

                var state = (PaymentState)request.PaymentState;
                var result = await _unitOfWork.StudentPaymentService.UpdatePaymentStateAsync(id, state, request.RejectMessage);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "UpdateState",
                        entityType: "StudentPayment",
                        entityId: id.ToString(),
                        message: $"Payment state updated to {state}",
                        ipAddress: GetClientIpAddress(),
                        details: state == PaymentState.Rejected ? $"Reason: {request.RejectMessage}" : null
                    );

                    await _hubContext.Clients.Group("Approvers").SendAsync("PaymentStateChanged", new
                    {
                        paymentId = id,
                        newState = state.ToString(),
                        updatedBy = User.Identity?.Name ?? "Unknown"
                    });

                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment state");
                return Json(new { success = false, message = "Error updating payment state" });
            }
        }

        [HttpGet("studentpayments/evidence-required")]
        public IActionResult IsEvidenceRequired()
        {
            // This is checked server-side in CreatePaymentAsync;
            // the client calls this to conditionally show the upload field
            return Json(new { required = true });
        }

        [HttpGet("studentpayments/pending-notifications")]
        public async Task<IActionResult> GetPendingPaymentNotifications()
        {
            try
            {
                var hasReportPerm = await _permissionService.UserHasPermissionAsync(User, SD.Permissions.REPORT);
                if (!hasReportPerm)
                    return Json(new { success = true, data = Array.Empty<object>(), count = 0 });

                var notifications = await _unitOfWork.StudentPaymentService.GetPendingPaymentNotificationsAsync();
                return Json(new { success = true, data = notifications, count = notifications.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending payment notifications");
                return Json(new { success = true, data = Array.Empty<object>(), count = 0 });
            }
        }

        #endregion

        #region Dropdown Helpers

        [HttpGet("dropdown/sessions")]
        public async Task<IActionResult> GetSessionsDropdown()
        {
            try
            {
                var items = await _viewsSelection.GetSessionsForDropdownAsync();
                var result = items.Select(i => new { id = i.Value, name = i.Text });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sessions dropdown");
                return Json(new List<object>());
            }
        }

        [HttpGet("dropdown/classes")]
        public async Task<IActionResult> GetClassesDropdown()
        {
            try
            {
                var items = await _viewsSelection.GetSchoolClassesForDropdownAsync();
                var result = items.Select(i => new { id = i.Value, name = i.Text });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading classes dropdown");
                return Json(new List<object>());
            }
        }

        [HttpGet("dropdown/subclasses")]
        public async Task<IActionResult> GetSubClassesDropdown()
        {
            try
            {
                var items = await _viewsSelection.GetSchoolSubclassesForDropdownAsync();
                var result = items.Select(i => new { id = i.Value, name = i.Text });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sub-classes dropdown");
                return Json(new List<object>());
            }
        }

        #endregion

        #region Dashboard

        [HttpGet("dashboard/category-summary")]
        public async Task<IActionResult> GetCategoryPaymentSummary()
        {
            try
            {
                var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
                if (settings == null || settings.sessionId == 0 || settings.term == 0)
                    return Ok(new { success = false, message = "App settings not configured" });

                var data = await _unitOfWork.PaymentReportService.GetDashboardCategorySummaryAsync(settings.sessionId, settings.term);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category payment summary");
                return Ok(new { success = false, message = "Error fetching data" });
            }
        }

        [HttpGet("dashboard/item-summary")]
        public async Task<IActionResult> GetItemPaymentSummary()
        {
            try
            {
                var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
                if (settings == null || settings.sessionId == 0 || settings.term == 0)
                    return Ok(new { success = false, message = "App settings not configured" });

                var data = await _unitOfWork.PaymentReportService.GetDashboardItemSummaryAsync(settings.sessionId, settings.term);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item payment summary");
                return Ok(new { success = false, message = "Error fetching data" });
            }
        }

        [HttpGet("dashboard/category-trend")]
        public async Task<IActionResult> GetCategoryPaymentTrend()
        {
            try
            {
                var data = await _unitOfWork.PaymentReportService.GetDashboardCategoryTrendAsync(10);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category payment trend");
                return Ok(new { success = false, message = "Error fetching data" });
            }
        }

        [HttpGet("dashboard/item-chart")]
        public async Task<IActionResult> GetItemPaymentChart()
        {
            try
            {
                var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
                if (settings == null || settings.sessionId == 0 || settings.term == 0)
                    return Ok(new { success = false, message = "App settings not configured" });

                var data = await _unitOfWork.PaymentReportService.GetDashboardItemChartAsync(settings.sessionId, settings.term);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item payment chart");
                return Ok(new { success = false, message = "Error fetching data" });
            }
        }

        [HttpGet("dashboard/term-registration-chart")]
        public async Task<IActionResult> GetTermRegistrationChart()
        {
            try
            {
                var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
                if (settings == null || settings.sessionId == 0)
                    return Ok(new { success = false, message = "App settings not configured" });

                var data = await _unitOfWork.PaymentReportService.GetDashboardTermRegistrationChartAsync(settings.sessionId);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching term registration chart");
                return Ok(new { success = false, message = "Error fetching data" });
            }
        }

        #endregion
    }
}
