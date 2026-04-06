using Microsoft.AspNetCore.Identity;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Custom role entity extending Identity
    /// Represents system roles: Admin, Accountant, Cashier
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
