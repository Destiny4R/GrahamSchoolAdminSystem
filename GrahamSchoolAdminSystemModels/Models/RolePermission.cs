using System.ComponentModel.DataAnnotations.Schema;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Many-to-many relationship between Roles and Permissions
    /// Defines which permissions each role has
    /// </summary>
    [Table("RolePermissions")]
    public class RolePermission
    {
        public string RoleId { get; set; }
        public int PermissionId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual ApplicationRole Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }
    }
}
