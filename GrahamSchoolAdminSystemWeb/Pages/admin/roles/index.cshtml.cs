using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.roles
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RoleWithPermissions> RolesWithPermissions { get; set; } = new List<RoleWithPermissions>();
        public List<Permission> AllPermissions { get; set; } = new List<Permission>();

        public async Task OnGetAsync()
        {
            // Load all permissions
            AllPermissions = await _context.Permissions.OrderBy(p => p.Name).ToListAsync();

            // Load all roles with their permissions
            var roles = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .OrderBy(r => r.Name)
                .ToListAsync();

            RolesWithPermissions = roles.Select(r => new RoleWithPermissions
            {
                RoleId = r.Id,
                RoleName = r.Name ?? "Unknown",
                Permissions = r.RolePermissions
                    .Select(rp => rp.Permission.Name)
                    .OrderBy(p => p)
                    .ToList()
            }).ToList();
        }
    }

    public class RoleWithPermissions
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
