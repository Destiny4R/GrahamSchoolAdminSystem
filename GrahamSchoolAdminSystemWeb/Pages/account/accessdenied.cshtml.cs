using GrahamSchoolAdminSystemAccess.IServiceRepo;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.account
{
    public class AccessDeniedModel : PageModel
    {
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string AttemptedResource { get; set; }
        public string RequiredPermission { get; set; }
        public DateTime AttemptTime { get; set; }
        public string IncidentId { get; set; }

        public AccessDeniedModel(ILogService logService, IHttpContextAccessor httpContextAccessor)
        {
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnGet()
        {
            AttemptTime = DateTime.UtcNow;
            IncidentId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();

            try
            {
                // Extract attempted resource from referrer or return URL
                var referrer = Request.Headers["Referer"].ToString();
                var returnUrl = Request.Query["returnUrl"].ToString();
                
                AttemptedResource = !string.IsNullOrEmpty(returnUrl) 
                    ? returnUrl 
                    : (!string.IsNullOrEmpty(referrer) ? new Uri(referrer).PathAndQuery : "Unknown");

                // Extract required permission from query string if provided
                RequiredPermission = Request.Query["permission"].ToString();

                // Get current user info
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
                var userName = User.Identity?.Name ?? "Anonymous";
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
                var userRoles = string.Join(", ", User.FindAll(ClaimTypes.Role).Select(c => c.Value));

                // Get client IP
                var clientIp = GetClientIpAddress();

                // Get additional context
                var httpMethod = Request.Method;
                var userAgent = Request.Headers["User-Agent"].ToString();
                var controller = HttpContext.GetRouteValue("controller")?.ToString() ?? "Unknown";
                var action = HttpContext.GetRouteValue("action")?.ToString() ?? "Unknown";

                // Build detailed message
                var message = $"Access Denied for user: {userName} attempting to access {AttemptedResource}";
                var details = $@"User ID: {userId}
Email: {userEmail}
Roles: {userRoles}
Attempted Resource: {AttemptedResource}
Required Permission: {RequiredPermission ?? "Not Specified"}
HTTP Method: {httpMethod}
Controller/Action: {controller}/{action}
User Agent: {userAgent}
Referrer: {referrer}
Return URL: {returnUrl}
Incident ID: {IncidentId}";

                // Log access denied attempt
                await _logService.LogUserActionAsync(
                    userId: userId,
                    userName: userName,
                    action: "AccessDenied",
                    entityType: "Authorization",
                    entityId: IncidentId,
                    message: message,
                    ipAddress: clientIp,
                    details: details
                );

                // Also log as warning if user has roles but was still denied
                if (!string.IsNullOrEmpty(userRoles) && userRoles != "")
                {
                    await _logService.LogActionAsync(
                        userId: userId,
                        userName: userName,
                        action: "AccessDeniedWithRoles",
                        entityType: "Authorization",
                        entityId: IncidentId,
                        ipAddress: clientIp,
                        subject: "Access Denied - User has roles",
                        message: $"User {userName} with roles [{userRoles}] was denied access to {AttemptedResource}",
                        logLevel: "WARNING",
                        details: details
                    );
                }
            }
            catch (Exception ex)
            {
                // Log the error in logging itself
                await _logService.LogErrorAsync(
                    subject: "Access Denied Page - Logging Error",
                    message: "Error while logging access denied attempt",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System",
                    ipAddress: GetClientIpAddress()
                );
            }
        }

        /// <summary>
        /// Get client IP address from HttpContext
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext == null)
                    return "Unknown";

                // Check for X-Forwarded-For header (for proxies)
                if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
                    if (!string.IsNullOrEmpty(forwardedFor))
                        return forwardedFor.Split(',')[0].Trim();
                }

                // Fall back to RemoteIpAddress
                return httpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
