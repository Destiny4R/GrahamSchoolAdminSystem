using System.ComponentModel.DataAnnotations.Schema;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Many-to-many relationship between Users and Roles
    /// Users can have multiple roles (e.g., both Admin and Accountant)
    /// </summary>
    [Table("UserRoles")]
    public class UserRole
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public string AssignedBy { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("RoleId")]
        public virtual ApplicationRole Role { get; set; }
    }
}
