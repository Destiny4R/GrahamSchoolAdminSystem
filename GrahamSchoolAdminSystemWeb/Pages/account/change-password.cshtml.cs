using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GrahamSchoolAdminSystemWeb.Pages.account
{
    [Authorize]
    public class change_passwordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUsersServices _usersServices;
        private readonly ILogService _logService;
        private readonly ILogger<change_passwordModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty]
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match")]
        public string ConfirmNewPassword { get; set; }

        public string UserEmail { get; set; }

        public change_passwordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersServices usersServices,
            ILogService logService,
            ILogger<change_passwordModel> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usersServices = usersServices;
            _logService = logService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/account/login");

            UserEmail = user.Email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/account/login");

            UserEmail = user.Email;

            if (!ModelState.IsValid)
                return Page();

            var ipAddress = GetClientIpAddress();

            var result = await _usersServices.ChangePasswordAsync(user.Id, CurrentPassword, NewPassword);

            if (result.Succeeded)
            {
                // Refresh sign-in so the security stamp cookie is updated
                await _signInManager.RefreshSignInAsync(user);

                await _logService.LogAuthenticationAsync(
                    userId: user.Id,
                    userName: user.UserName,
                    action: "ChangePassword",
                    ipAddress: ipAddress,
                    success: true,
                    message: $"User {user.UserName} changed their password successfully",
                    details: $"Password changed from IP: {ipAddress}"
                );

                await _logService.LogUserActionAsync(
                    userId: user.Id,
                    userName: user.UserName,
                    action: "ChangePassword",
                    entityType: "Authentication",
                    entityId: user.Id,
                    message: $"User {user.UserName} changed their password",
                    ipAddress: ipAddress,
                    details: "Password changed successfully"
                );

                _logger.LogInformation("User {UserName} changed their password from IP: {IpAddress}", user.UserName, ipAddress);

                TempData["Success"] = "Your password has been changed successfully.";
                return RedirectToPage();
            }

            await _logService.LogAuthenticationAsync(
                userId: user.Id,
                userName: user.UserName,
                action: "ChangePassword",
                ipAddress: ipAddress,
                success: false,
                message: $"Password change failed for user {user.UserName}",
                details: result.Message
            );

            _logger.LogWarning("Password change failed for user {UserName}: {Message}", user.UserName, result.Message);
            TempData["Error"] = result.Message;
            return Page();
        }

        private string GetClientIpAddress()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

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
