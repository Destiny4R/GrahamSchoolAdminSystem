using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrahamSchoolAdminSystemWeb.Pages.account
{
    [Authorize]
    public class logoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly ILogger<logoutModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public logoutModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogService logService,
            ILogger<logoutModel> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logService = logService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get user info before sign out
            var user = await _userManager.GetUserAsync(User);
            var ipAddress = GetClientIpAddress();
            var userId = user?.Id;
            var userName = user?.UserName ?? User.Identity?.Name ?? "Unknown";

            try
            {
                // Log the logout action before signing out
                if (user != null)
                {
                    await _logService.LogAuthenticationAsync(
                        userId: userId,
                        userName: userName,
                        action: "Logout",
                        ipAddress: ipAddress,
                        success: true,
                        message: $"User {userName} logged out successfully",
                        details: $"User initiated logout from IP: {ipAddress}"
                    );

                    // Log user activity
                    await _logService.LogUserActionAsync(
                        userId: userId,
                        userName: userName,
                        action: "Logout",
                        entityType: "Authentication",
                        entityId: userId,
                        message: $"User {userName} signed out",
                        ipAddress: ipAddress,
                        details: "User initiated logout"
                    );

                    _logger.LogInformation($"User {userName} logged out from IP: {ipAddress}");
                }

                // Sign out the user
                await _signInManager.SignOutAsync();

                // Set success message for login page
                TempData["SuccessMessage"] = "You have been logged out successfully.";
                TempData["LogoutSuccess"] = true;

                return RedirectToPage("/account/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during logout for user: {userName}");

                await _logService.LogErrorAsync(
                    subject: "Logout Error",
                    message: $"An error occurred during logout for user {userName}",
                    details: ex.Message,
                    userId: userId,
                    ipAddress: ipAddress
                );

                // Still try to sign out even if logging fails
                await _signInManager.SignOutAsync();

                TempData["ErrorMessage"] = "An error occurred during logout, but you have been signed out.";
                return RedirectToPage("/account/login");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Handle POST requests the same way as GET
            return await OnGetAsync();
        }

        /// <summary>
        /// Get client IP address from request
        /// </summary>
        private string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

            // Check for IP forwarded by proxy
            if (httpContext?.Request.Headers.ContainsKey("X-Forwarded-For") == true)
            {
                var forwardedIp = httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwardedIp))
                {
                    ipAddress = forwardedIp.Trim();
                }
            }

            return ipAddress ?? "Unknown";
        }
    }
}
