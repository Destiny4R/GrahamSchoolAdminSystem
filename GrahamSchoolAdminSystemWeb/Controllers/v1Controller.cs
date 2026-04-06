using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class v1Controller : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<v1Controller> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;

        public v1Controller(
            IUnitOfWork unitOfWork, 
            ILogger<v1Controller> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IPermissionService permissionService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _permissionService = permissionService;
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

        #region Fees Setup

        /// GET: api/v1/feessetup/selections - Get fees setup dropdown selections
        /// </summary>
        [HttpGet("feessetup/selections")]
        public async Task<IActionResult> GetFeesSetupSelections()
        {
            try
            {
                var selections = await _unitOfWork.FinanceServices.GetFeesSetupSelectionsAsync();
                return Json(new { success = true, data = selections });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fees setup selections");
                return Json(new { success = false, message = "Error loading selections" });
            }
        }

        /// <summary>
        /// POST: api/v1/feessetup/create - Create fees setup
        /// </summary>
        [HttpPost("feessetup/create")]
        public async Task<IActionResult> CreateFeesSetup([FromBody] FeesSetupViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.FinanceServices.CreateFeesSetupAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Create",
                        entityType: "FeesSetup",
                        entityId: result.Data.ToString(),
                        message: $"Fees setup created - Amount: {model.Amount}, Term: {model.Term}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}"
                    );

                    return Json(new { success = true, message = result.Message, id = result.Data });
                }

                return Json(new { success = false, message = result.Message });
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

                return Json(new { success = false, message = "Error creating fees setup" });
            }
        }

        /// <summary>
        /// GET: api/v1/feessetup/5 - Get fees setup by ID for editing
        /// </summary>
        [HttpGet("feessetup/{id}")]
        public async Task<IActionResult> GetFeesSetup(int id)
        {
            try
            {
                var feesSetup = await _unitOfWork.FinanceServices.GetFeesSetupByIdAsync(id);
                if (feesSetup == null)
                    return Json(new { error = "Fees setup not found" });

                return Json(new { success = true, data = feesSetup });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fees setup");
                return Json(new { error = "Error loading data" });
            }
        }

        /// <summary>
        /// PUT: api/v1/feessetup/update - Update fees setup
        /// </summary>
        [HttpPut("feessetup/update")]
        public async Task<IActionResult> UpdateFeesSetup([FromBody] FeesSetupViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid form data" });

            try
            {
                var result = await _unitOfWork.FinanceServices.UpdateFeesSetupAsync(model);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Update",
                        entityType: "FeesSetup",
                        entityId: model.Id.ToString(),
                        message: $"Fees setup updated - Amount: {model.Amount}, Term: {model.Term}",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fees setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Fees Setup Update Error",
                    message: "Error updating fees setup",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error updating fees setup" });
            }
        }

        /// <summary>
        /// DELETE: api/v1/feessetup/5 - Delete fees setup (with SweetAlert2 confirmation)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteFeesSetup(int id)
        {
            try
            {
                var result = await _unitOfWork.FinanceServices.DeleteFeesSetupAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Delete",
                        entityType: "FeesSetup",
                        entityId: id.ToString(),
                        message: "Fees setup deleted successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Deleted fees setup with ID: {id}"
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fees setup");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Fees Setup Delete Error",
                    message: "Error deleting fees setup",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting fees setup" });
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

        #endregion

        #region PTA FEES SET UP
        [HttpGet("ptasetuppayment/{id}")]
        public async Task<IActionResult> GetPTAFeesSetupById(int id)
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

        #endregion

        #region Other Payment Items
        [HttpDelete("otherpaymentitems/{id}")]
        public async Task<IActionResult> DeleteOtherPaymentItem(int id)
        {
            try
            {
                if (id < 1)
                    return Json(new { success = false, message = "Invalid payment item" });

                var result = await _unitOfWork.OtherPaymentServices.DeleteOtherItemAsync(id);

                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "DeleteMultiple",
                        entityType: "OtherPaymentItem",
                        entityId: id.ToString(),
                        message: $"Deleted other payment item with ID {id}",
                        ipAddress: GetClientIpAddress()
                    );

                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting other payment item");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Other Payment Item Delete Error",
                    message: "Error deleting other payment item",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                return Json(new { success = false, message = "Error deleting other payment item" });
            }
        }

        [HttpGet("othersetuppayment/{id}")]
        public async Task<IActionResult> GetOtherFeesSetupById(int id)
        {
            try
            {
                var otherFeesSetup = await _unitOfWork.OtherPaymentServices.GetOtherFeesSetUpByIdAsync(id);
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

        [HttpDelete("DeleteOtherFeesSetup/{id}")]
        public async Task<IActionResult> DeleteOtherFeesSetupAsync(int id)
        {
            try
            {
                var otherFeesSetup = await _unitOfWork.FinanceServices.DeleteOtherFeesSetupAsync(id);
                if (otherFeesSetup == null)
                    return new JsonResult(new { succeeded = false, message = "Unknown request" });

                return new JsonResult(new { succeeded = otherFeesSetup.Succeeded, message = otherFeesSetup.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving other fees setup");
                return new JsonResult(new { succeeded = false, message = "An error occurred while processing this request" });
            }
        }

        #endregion
    }
}
