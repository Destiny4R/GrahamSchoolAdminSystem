using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class LogsTable
    {
        public int Id { get; set; }

        [StringLength(54)]
        public string Subject { get; set; }

        [StringLength(2000)]
        public string Message { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Log level: INFO, WARNING, ERROR, CRITICAL, DEBUG
        /// </summary>
        [StringLength(20)]
        public string LogLevel { get; set; } = "INFO";

        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Username of the user who performed the action
        /// </summary>
        [StringLength(256)]
        public string UserName { get; set; }

        /// <summary>
        /// Action performed (Create, Update, Delete, Read, Login, etc.)
        /// </summary>
        [StringLength(50)]
        public string Action { get; set; }

        /// <summary>
        /// Entity type affected (Position, Role, User, etc.)
        /// </summary>
        [StringLength(100)]
        public string EntityType { get; set; }

        /// <summary>
        /// ID of the entity affected
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// IP Address of the client
        /// </summary>
        [StringLength(45)]
        public string IpAddress { get; set; }

        /// <summary>
        /// Additional data/details in JSON format
        /// </summary>
        [StringLength(4000)]
        public string Details { get; set; }

        /// <summary>
        /// Response status code if applicable
        /// </summary>
        public int? StatusCode { get; set; }
    }
}
