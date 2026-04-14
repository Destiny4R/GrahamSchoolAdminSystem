using Microsoft.AspNetCore.Identity;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Custom role entity extending Identity
    /// Represents system roles: Admin, Accountant, Cashier
    /// Assigned to Positions via PositionRole, then to Employees via Position
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<PositionRole> PositionRoles { get; set; } = new List<PositionRole>();
    }
}
