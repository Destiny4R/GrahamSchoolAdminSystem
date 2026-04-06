
using Microsoft.AspNetCore.Identity;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Custom user entity extending Identity
    /// Users can have multiple roles assigned via UserRole junction table
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
