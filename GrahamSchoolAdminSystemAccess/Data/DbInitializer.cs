using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize database with seed data for roles, positions, and admin user
        /// </summary>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully");

                // Seed roles
                await SeedRoles(roleManager, logger);

                // Seed permissions and role-permission mappings
                await RolesPermissionsSeeder.SeedAsync(context, roleManager);
                logger.LogInformation("Roles and permissions seeded successfully");

                // Seed positions
                await SeedPositions(context, logger);

                // Seed admin user
                await SeedAdminUser(userManager, context, logger);

                // Assign admin role directly to admin user via UserRole table
                await AssignAdminRole(userManager, context, logger);

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding");
                throw;
            }
        }

        /// <summary>
        /// Seed default roles (Admin, Account, Cashier)
        /// </summary>
        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            var roles = new[] { SD.Roles.ADMIN, SD.Roles.ACCOUNT, SD.Roles.CASHIER };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Role '{roleName}' already exists");
                }
            }
        }

        /// <summary>
        /// Seed example positions (optional - admins can create their own)
        /// </summary>
        private static async Task SeedPositions(ApplicationDbContext context, ILogger logger)
        {
            // Check if positions already exist
            if (await context.PositionTables.AnyAsync())
            {
                logger.LogInformation("Positions already exist in database");
                return;
            }

            // Optional: Seed a few example positions
            // Admins can delete these and create their own positions via the UI
            var positions = new List<PositionTable>
            {
                new PositionTable
                {
                    Name = "Principal",
                    Description = "School Principal - Chief administrative officer",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new PositionTable
                {
                    Name = "Teacher",
                    Description = "Teacher - Responsible for instruction and student development",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };

            await context.PositionTables.AddRangeAsync(positions);
            await context.SaveChangesAsync();
            logger.LogInformation($"Seeded {positions.Count} example positions (can be modified by admin)");
        }

        /// <summary>
        /// Create admin user with full permissions
        /// </summary>
        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger logger)
        {
            const string adminEmail = "admin@grahamschool.com";
            const string adminPassword = "Admin@123456";
            const string adminUserName = "admin";

            // Check if admin user already exists
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                logger.LogInformation($"Admin user '{adminEmail}' already exists");
                return;
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = false
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Assign Admin role to the user
                var roleResult = await userManager.AddToRoleAsync(adminUser, SD.Roles.ADMIN);
                if (roleResult.Succeeded)
                {
                    logger.LogInformation($"Admin user '{adminEmail}' created and assigned Admin role successfully");
                    logger.LogInformation($"Default admin credentials - Email: {adminEmail}, Password: {adminPassword}");
                }
                else
                {
                    logger.LogError($"Failed to assign Admin role to user '{adminEmail}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        /// <summary>
        /// Assign Admin role to admin user via UserRole table
        /// </summary>
        private static async Task AssignAdminRole(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger logger)
        {
            const string adminEmail = "admin@grahamschool.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                logger.LogWarning("Admin user not found for role assignment");
                return;
            }

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SD.Roles.ADMIN);
            if (adminRole == null)
            {
                logger.LogWarning("Admin role not found");
                return;
            }

            // Check if UserRole entry already exists
            var existingUserRole = await context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

            if (existingUserRole == null)
            {
                var userRole = new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    AssignedDate = DateTime.UtcNow,
                    AssignedBy = "System"
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
                logger.LogInformation($"Admin role assigned to user '{adminEmail}' via UserRole table");
            }
            else
            {
                logger.LogInformation($"Admin role already assigned to user '{adminEmail}'");
            }
        }

        /// <summary>
        /// Assign all roles to the School Principal position (DEPRECATED - keeping for backward compatibility)
        /// </summary>
        [Obsolete("This method is deprecated. Roles should be assigned to users, not positions.")]
        private static async Task AssignRolesToPositions(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Get the Principal position
                var principalPosition = await context.PositionTables.FirstOrDefaultAsync(p => p.Name == "School Principal");
                if (principalPosition == null)
                {
                    logger.LogWarning("School Principal position not found");
                    return;
                }

                // Check if roles are already assigned
                var existingAssignments = await context.PositionRoles.Where(pr => pr.PositionId == principalPosition.Id).ToListAsync();
                if (existingAssignments.Count > 0)
                {
                    logger.LogInformation("Roles already assigned to School Principal position");
                    return;
                }

                // Get all roles
                var roles = await context.Roles.ToListAsync();

                // Assign all roles to School Principal
                var positionRoles = new List<PositionRole>();
                foreach (var role in roles)
                {
                    positionRoles.Add(new PositionRole
                    {
                        PositionId = principalPosition.Id,
                        RoleId = role.Id
                    });
                }

                await context.PositionRoles.AddRangeAsync(positionRoles);
                await context.SaveChangesAsync();
                logger.LogInformation($"Assigned {positionRoles.Count} roles to School Principal position");

                // Assign other roles to their respective positions
                await AssignRoleToPosition(context, "Finance Manager", SD.Roles.ACCOUNT, logger);
                await AssignRoleToPosition(context, "Cashier", SD.Roles.CASHIER, logger);
                await AssignRoleToPosition(context, "Accountant", SD.Roles.ACCOUNT, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error assigning roles to positions");
            }
        }

        /// <summary>
        /// Helper method to assign a single role to a position
        /// </summary>
        private static async Task AssignRoleToPosition(ApplicationDbContext context, string positionName, string roleName, ILogger logger)
        {
            var position = await context.PositionTables.FirstOrDefaultAsync(p => p.Name == positionName);
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (position == null || role == null)
            {
                logger.LogWarning($"Position '{positionName}' or Role '{roleName}' not found");
                return;
            }

            // Check if already assigned
            var exists = await context.PositionRoles.AnyAsync(pr => pr.PositionId == position.Id && pr.RoleId == role.Id);
            if (!exists)
            {
                await context.PositionRoles.AddAsync(new PositionRole
                {
                    PositionId = position.Id,
                    RoleId = role.Id
                });
                await context.SaveChangesAsync();
                logger.LogInformation($"Assigned role '{roleName}' to position '{positionName}'");
            }
        }
    }
}
