using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GrahamSchoolAdminSystemWeb.Pages.account
{
    public class loginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly ILogger<loginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty]
        [EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public loginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogService logService,
            ILogger<loginModel> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logService = logService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnGetAsync()
        {
            // If user is already logged in, redirect to dashboard
            if (User?.Identity?.IsAuthenticated == true)
            {
                RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please enter valid credentials.";
                return Page();
            }

            try
            {
                var ipAddress = GetClientIpAddress();
                var user = await _userManager.FindByEmailAsync(Email);

                if (user == null)
                {
                    // Log failed login attempt - user not found
                    await _logService.LogAuthenticationAsync(
                        userId: null,
                        userName: Email,
                        action: "Login",
                        ipAddress: ipAddress,
                        success: false,
                        message: "Login attempt failed - user not found",
                        details: $"Email: {Email}"
                    );

                    _logger.LogWarning($"Login attempt for non-existent user: {Email} from IP: {ipAddress}");
                    ErrorMessage = "Invalid email or password.";
                    return Page();
                }

                // Check if user account is enabled
                if (user.LockoutEnabled && await _userManager.IsLockedOutAsync(user))
                {
                    await _logService.LogAuthenticationAsync(
                        userId: user.Id,
                        userName: user.UserName,
                        action: "Login",
                        ipAddress: ipAddress,
                        success: false,
                        message: "Login attempt failed - account locked out",
                        details: $"Account is locked due to multiple failed login attempts"
                    );

                    _logger.LogWarning($"Login attempt for locked user: {Email} from IP: {ipAddress}");
                    ErrorMessage = "Your account has been locked due to multiple failed login attempts. Please try again later.";
                    return Page();
                }

                // Attempt sign in
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    Password,
                    isPersistent: RememberMe,
                    lockoutOnFailure: true
                );

                if (result.Succeeded)
                {
                    // Log successful login
                    await _logService.LogAuthenticationAsync(
                        userId: user.Id,
                        userName: user.UserName,
                        action: "Login",
                        ipAddress: ipAddress,
                        success: true,
                        message: $"User {user.UserName} logged in successfully",
                        details: $"Email: {user.Email}, RememberMe: {RememberMe}"
                    );

                    _logger.LogInformation($"User {user.UserName} logged in successfully from IP: {ipAddress}");

                    // Log user activity
                    await _logService.LogUserActionAsync(
                        userId: user.Id,
                        userName: user.UserName,
                        action: "Login",
                        entityType: "Authentication",
                        entityId: user.Id,
                        message: $"User {user.UserName} successfully authenticated",
                        ipAddress: ipAddress,
                        details: $"Login successful - RememberMe: {RememberMe}"
                    );

                    return RedirectToPage("/Index");
                }

                if (result.IsLockedOut)
                {
                    await _logService.LogAuthenticationAsync(
                        userId: user.Id,
                        userName: user.UserName,
                        action: "Login",
                        ipAddress: ipAddress,
                        success: false,
                        message: "Account locked due to multiple failed attempts"
                    );

                    _logger.LogWarning($"User account locked: {Email}");
                    ErrorMessage = "Your account has been locked. Please try again later.";
                    return Page();
                }

                // Failed login attempt
                await _logService.LogAuthenticationAsync(
                    userId: user.Id,
                    userName: user.UserName,
                    action: "Login",
                    ipAddress: ipAddress,
                    success: false,
                    message: "Invalid password",
                    details: $"Failed login attempt for user {user.UserName}"
                );

                _logger.LogWarning($"Failed login attempt for user: {Email} from IP: {ipAddress}");
                ErrorMessage = "Invalid email or password.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user: {Email}");

                await _logService.LogErrorAsync(
                    subject: "Login Error",
                    message: $"An error occurred during login attempt for {Email}",
                    details: ex.Message,
                    ipAddress: GetClientIpAddress()
                );

                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
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
                if (!string.IsNullOrEmpty(forwardedIp))
                {
                    ipAddress = forwardedIp.Trim();
                }
            }

            return ipAddress ?? "Unknown";
        }
    }
}
