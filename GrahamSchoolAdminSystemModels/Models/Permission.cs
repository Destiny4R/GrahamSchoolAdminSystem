using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Represents system permissions (Create, Edit, Delete, View)
    /// </summary>
    [Table("Permissions")]
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } // Create, Edit, Delete, View

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
