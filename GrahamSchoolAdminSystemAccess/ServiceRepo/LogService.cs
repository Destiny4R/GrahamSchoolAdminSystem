using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _db;

        public LogService(ApplicationDbContext db)
        {
            this._db = db;
        }

        /// <summary>
        /// Simple synchronous logging
        /// </summary>
        public void Log(string subject, string message)
        {
            try
            {
                var logs = new LogsTable
                {
                    Subject = subject,
                    Message = message,
                    LogLevel = "INFO",
                    CreatedDate = DateTime.UtcNow
                };
                _db.Add(logs);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Simple asynchronous logging
        /// </summary>
        public async Task LogAsync(string subject, string message)
        {
            try
            {
                var logs = new LogsTable
                {
                    Subject = subject,
                    Message = message,
                    LogLevel = "INFO",
                    CreatedDate = DateTime.UtcNow
                };
                _db.Add(logs);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Log an action with full audit details
        /// </summary>
        public async Task LogActionAsync(
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
            int? statusCode = null)
        {
            try
            {
                var log = new LogsTable
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    Subject = subject,
                    Message = message,
                    LogLevel = logLevel,
                    Details = details,
                    StatusCode = statusCode,
                    CreatedDate = DateTime.UtcNow
                };

                _db.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a user action (Create, Update, Delete)
        /// </summary>
        public async Task LogUserActionAsync(
            string userId,
            string userName,
            string action,
            string entityType,
            string entityId,
            string message,
            string ipAddress,
            string details = null)
        {
            await LogActionAsync(
                userId,
                userName,
                action,
                entityType,
                entityId,
                ipAddress,
                $"{action} - {entityType}",
                message,
                "INFO",
                details
            );
        }

        /// <summary>
        /// Log authentication events (Login, Logout, Failed Login)
        /// </summary>
        public async Task LogAuthenticationAsync(
            string userId,
            string userName,
            string action,
            string ipAddress,
            bool success,
            string message = null,
            string details = null)
        {
            var logLevel = success ? "INFO" : "WARNING";
            var subject = $"Authentication - {action}";
            var defaultMessage = success 
                ? $"User '{userName}' {action.ToLower()} successfully"
                : $"Failed {action.ToLower()} attempt for user '{userName}'";

            await LogActionAsync(
                userId ?? "Anonymous",
                userName ?? "Unknown",
                action,
                "Authentication",
                userId,
                ipAddress,
                subject,
                message ?? defaultMessage,
                logLevel,
                details,
                success ? 200 : 401
            );
        }

        /// <summary>
        /// Log an error
        /// </summary>
        public async Task LogErrorAsync(
            string subject,
            string message,
            string details = null,
            string userId = null,
            string ipAddress = null)
        {
            try
            {
                var log = new LogsTable
                {
                    Subject = subject,
                    Message = message,
                    Details = details,
                    UserId = userId,
                    IpAddress = ipAddress,
                    LogLevel = "ERROR",
                    CreatedDate = DateTime.UtcNow
                };

                _db.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get logs with filtering
        /// </summary>
        public async Task<List<dynamic>> GetLogsAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string searchTerm = null,
            string logLevel = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _db.LogsTables.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(l => 
                        l.Subject.Contains(searchTerm) ||
                        l.Message.Contains(searchTerm) ||
                        l.UserName.Contains(searchTerm) ||
                        l.EntityType.Contains(searchTerm)
                    );
                }

                if (!string.IsNullOrEmpty(logLevel))
                {
                    query = query.Where(l => l.LogLevel == logLevel);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.CreatedDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.CreatedDate <= endDate.Value.AddDays(1));
                }

                var logs = await query
                    .OrderByDescending(l => l.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.Subject,
                        l.Message,
                        l.CreatedDate,
                        l.LogLevel,
                        l.UserName,
                        l.Action,
                        l.EntityType,
                        l.EntityId,
                        l.IpAddress
                    })
                    .ToListAsync();

                return logs.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving logs: {ex.Message}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Get logs by user
        /// </summary>
        public async Task<List<dynamic>> GetUserLogsAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var logs = await _db.LogsTables
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.Subject,
                        l.Message,
                        l.CreatedDate,
                        l.LogLevel,
                        l.Action,
                        l.EntityType,
                        l.EntityId
                    })
                    .ToListAsync();

                return logs.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving user logs: {ex.Message}");
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Get activity logs for an entity
        /// </summary>
        public async Task<List<dynamic>> GetEntityLogsAsync(string entityType, string entityId)
        {
            try
            {
                var logs = await _db.LogsTables
                    .Where(l => l.EntityType == entityType && l.EntityId == entityId)
                    .OrderByDescending(l => l.CreatedDate)
                    .Select(l => new
                    {
                        l.Id,
                        l.Subject,
                        l.Message,
                        l.CreatedDate,
                        l.Action,
                        l.UserName,
                        l.IpAddress
                    })
                    .ToListAsync();

                return logs.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving entity logs: {ex.Message}");
                return new List<dynamic>();
            }
        }
    }
}
