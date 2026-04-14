
using Microsoft.AspNetCore.Identity;

namespace GrahamSchoolAdminSystemModels.Models
{
    /// <summary>
    /// Custom user entity extending Identity
    /// Roles are inherited through Employee → Position → PositionRole chain
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
    }
}
