using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemAccess.Data
{
    /// <summary>
    /// Seeds default roles, permissions, and role-permission mappings
    /// </summary>
    public static class RolesPermissionsSeeder
    {
        /// <summary>
        /// Seeds the database with default roles, permissions, and their mappings
        /// </summary>
        public static async Task SeedAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            // Seed Permissions
            await SeedPermissionsAsync(context);

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Role-Permission mappings
            await SeedRolePermissionsAsync(context);
        }

        private static async Task SeedPermissionsAsync(ApplicationDbContext context)
        {
            var permissionsToSeed = new List<(string Name, string Description)>
            {
                ("Create", "Permission to create new records"),
                ("Edit", "Permission to edit existing records"),
                ("Delete", "Permission to delete records"),
                ("View", "Permission to view records"),
                ("Report", "Permission to generate and view reports")
            };

            foreach (var (name, description) in permissionsToSeed)
            {
                if (!await context.Permissions.AnyAsync(p => p.Name == name))
                {
                    context.Permissions.Add(new Permission
                    {
                        Name = name,
                        Description = description,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[] { "Admin", "Accountant", "Cashier" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole { Name = roleName };
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task SeedRolePermissionsAsync(ApplicationDbContext context)
        {
            // Only seed if no role-permission mappings exist yet (first-time setup only)
            if (await context.RolePermissions.AnyAsync())
                return;

            // Get roles and permissions
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var accountantRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Accountant");
            var cashierRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Cashier");

            var createPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Create");
            var editPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Edit");
            var deletePermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Delete");
            var viewPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "View");
            var reportPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Report");

            if (adminRole == null || accountantRole == null || cashierRole == null ||
                createPermission == null || editPermission == null || deletePermission == null || viewPermission == null)
            {
                throw new InvalidOperationException("Required roles or permissions not found in database");
            }

            var mappingsToSeed = new List<(string RoleId, int PermissionId)>
            {
                // Admin - Full permissions (Create, Edit, Delete, View, Report)
                (adminRole.Id, createPermission.Id),
                (adminRole.Id, editPermission.Id),
                (adminRole.Id, deletePermission.Id),
                (adminRole.Id, viewPermission.Id),

                // Accountant - Create, Edit, View, Report (no Delete)
                (accountantRole.Id, createPermission.Id),
                (accountantRole.Id, editPermission.Id),
                (accountantRole.Id, viewPermission.Id),

                // Cashier - View only
                (cashierRole.Id, viewPermission.Id)
            };

            // Add Report permission mappings if Report permission exists
            if (reportPermission != null)
            {
                mappingsToSeed.Add((adminRole.Id, reportPermission.Id));
                mappingsToSeed.Add((accountantRole.Id, reportPermission.Id));
            }

            foreach (var (roleId, permissionId) in mappingsToSeed)
            {
                if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId,
                        AssignedDate = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
