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
            if (await context.Permissions.AnyAsync())
            {
                return; // Permissions already seeded
            }

            var permissions = new List<Permission>
            {
                new Permission
                {
                    Name = "Create",
                    Description = "Permission to create new records",
                    CreatedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Edit",
                    Description = "Permission to edit existing records",
                    CreatedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Delete",
                    Description = "Permission to delete records",
                    CreatedDate = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "View",
                    Description = "Permission to view records",
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.Permissions.AddRangeAsync(permissions);
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
            if (await context.RolePermissions.AnyAsync())
            {
                return; // Role permissions already seeded
            }

            // Get roles and permissions
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var accountantRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Accountant");
            var cashierRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Cashier");

            var createPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Create");
            var editPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Edit");
            var deletePermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "Delete");
            var viewPermission = await context.Permissions.FirstOrDefaultAsync(p => p.Name == "View");

            if (adminRole == null || accountantRole == null || cashierRole == null ||
                createPermission == null || editPermission == null || deletePermission == null || viewPermission == null)
            {
                throw new InvalidOperationException("Required roles or permissions not found in database");
            }

            var rolePermissions = new List<RolePermission>
            {
                // Admin - Full permissions (Create, Edit, Delete, View)
                new RolePermission { RoleId = adminRole.Id, PermissionId = createPermission.Id, AssignedDate = DateTime.UtcNow },
                new RolePermission { RoleId = adminRole.Id, PermissionId = editPermission.Id, AssignedDate = DateTime.UtcNow },
                new RolePermission { RoleId = adminRole.Id, PermissionId = deletePermission.Id, AssignedDate = DateTime.UtcNow },
                new RolePermission { RoleId = adminRole.Id, PermissionId = viewPermission.Id, AssignedDate = DateTime.UtcNow },

                // Accountant - Create, Edit, View (no Delete)
                new RolePermission { RoleId = accountantRole.Id, PermissionId = createPermission.Id, AssignedDate = DateTime.UtcNow },
                new RolePermission { RoleId = accountantRole.Id, PermissionId = editPermission.Id, AssignedDate = DateTime.UtcNow },
                new RolePermission { RoleId = accountantRole.Id, PermissionId = viewPermission.Id, AssignedDate = DateTime.UtcNow },

                // Cashier - View only
                new RolePermission { RoleId = cashierRole.Id, PermissionId = viewPermission.Id, AssignedDate = DateTime.UtcNow }
            };

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
        }
    }
}
