using GrahamSchoolAdminSystemAccess.IServiceRepo;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Helpers
{
    /// <summary>
    /// Action filter for logging user activities across the application
    /// </summary>
    public class AuditLoggingFilter : IAsyncActionFilter
    {
        private readonly ILogService _logService;
        private readonly ILogger<AuditLoggingFilter> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLoggingFilter(
            ILogService logService,
            ILogger<AuditLoggingFilter> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logService = logService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var page = context.RouteData.Values["page"]?.ToString() ?? controller;

            var httpContext = context.HttpContext;
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = httpContext.User.Identity?.Name ?? "Anonymous";
            var ipAddress = GetClientIpAddress(httpContext);

            try
            {
                // Execute the action
                var resultContext = await next();

                // Log successful action
                if (IsAuditableAction(context.ActionDescriptor))
                {
                    var httpMethod = context.HttpContext.Request.Method;
                    var action = DetermineAction(httpMethod, actionName);

                    await _logService.LogActionAsync(
                        userId: userId ?? "Anonymous",
                        userName: userName,
                        action: action,
                        entityType: page,
                        entityId: GetEntityId(context),
                        ipAddress: ipAddress,
                        subject: $"{action} - {page}",
                        message: $"User {userName} performed {action} action",
                        logLevel: "INFO",
                        details: GetActionDetails(context),
                        statusCode: resultContext.HttpContext.Response.StatusCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging action: {actionName}");

                await _logService.LogErrorAsync(
                    subject: "Action Logging Error",
                    message: $"Error logging action {actionName}",
                    details: ex.Message,
                    userId: userId,
                    ipAddress: ipAddress
                );
            }
        }

        /// <summary>
        /// Determine if this action should be logged
        /// </summary>
        private bool IsAuditableAction(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor descriptor)
        {
            var methodName = descriptor.DisplayName;
            
            // Don't log GET requests by default (only log POST, PUT, DELETE, PATCH)
            if (methodName.Contains("OnGetAsync") || methodName.Contains("OnGet"))
            {
                return false;
            }

            // Skip authentication actions from auditing (they're logged separately)
            if (methodName.Contains("Login") || methodName.Contains("Logout") || methodName.Contains("Register"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determine the action type from HTTP method
        /// </summary>
        private string DetermineAction(string httpMethod, string actionName)
        {
            return httpMethod.ToUpper() switch
            {
                "GET" => "Read",
                "POST" => actionName.Contains("Add") || actionName.Contains("Create") ? "Create" : "Update",
                "PUT" => "Update",
                "DELETE" => "Delete",
                "PATCH" => "Update",
                _ => "Access"
            };
        }

        /// <summary>
        /// Get entity ID from route or form data
        /// </summary>
        private string GetEntityId(ActionExecutingContext context)
        {
            if (context.RouteData.Values.TryGetValue("id", out var id))
            {
                return id?.ToString();
            }

            foreach (var param in context.ActionArguments)
            {
                if (param.Key.Contains("id") && param.Value != null)
                {
                    return param.Value.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Get detailed information about the action
        /// </summary>
        private string GetActionDetails(ActionExecutingContext context)
        {
            try
            {
                var details = new System.Text.StringBuilder();
                details.AppendLine($"Controller: {context.RouteData.Values["controller"]}");
                details.AppendLine($"Action: {context.RouteData.Values["action"]}");
                details.AppendLine($"Method: {context.HttpContext.Request.Method}");

                if (context.ActionArguments.Count > 0)
                {
                    details.AppendLine("Parameters:");
                    foreach (var param in context.ActionArguments)
                    {
                        details.AppendLine($"  - {param.Key}: {param.Value?.ToString() ?? "null"}");
                    }
                }

                return details.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get client IP address from request
        /// </summary>
        private string GetClientIpAddress(HttpContext httpContext)
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            // Check for IP forwarded by proxy
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
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
