using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    /// <summary>
    /// Service for logging and auditing system activities
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Log a simple message with subject
        /// </summary>
        void Log(string subject, string message);

        /// <summary>
        /// Log an async simple message with subject
        /// </summary>
        Task LogAsync(string subject, string message);

        /// <summary>
        /// Log an action with full audit details
        /// </summary>
        Task LogActionAsync(
            string userId,
            string userName,
            string action,
            string entityType,
            string entityId,
            string ipAddress,
            string subject,
            string message,
            string logLevel = "INFO",
            string details = null,
            int? statusCode = null
        );

        /// <summary>
        /// Log a user action (Create, Update, Delete)
        /// </summary>
        Task LogUserActionAsync(
            string userId,
            string userName,
            string action,
            string entityType,
            string entityId,
            string message,
            string ipAddress,
            string details = null
        );

        /// <summary>
        /// Log authentication events (Login, Logout, Failed Login)
        /// </summary>
        Task LogAuthenticationAsync(
            string userId,
            string userName,
            string action,
            string ipAddress,
            bool success,
            string message = null,
            string details = null
        );

        /// <summary>
        /// Log an error
        /// </summary>
        Task LogErrorAsync(
            string subject,
            string message,
            string details = null,
            string userId = null,
            string ipAddress = null
        );

        /// <summary>
        /// Get logs with filtering
        /// </summary>
        Task<List<dynamic>> GetLogsAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string searchTerm = null,
            string logLevel = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        /// <summary>
        /// Get logs by user
        /// </summary>
        Task<List<dynamic>> GetUserLogsAsync(string userId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Get activity logs for an entity
        /// </summary>
        Task<List<dynamic>> GetEntityLogsAsync(string entityType, string entityId);
    }
}
