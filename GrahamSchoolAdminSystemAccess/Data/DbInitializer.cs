using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        /// Initialize database with seed data for roles, permissions, positions, and admin user.
        /// Authorization chain: Employee → Position → Role → Permission
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

                // Assign roles to positions
                await AssignRolesToPositions(context, logger);

                // Seed admin user with Position-based access
                await SeedAdminUser(userManager, context, logger);

                //Create application settings for the admin user
                await CreateAppSettings(context, logger);

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding");
                throw;
            }
        }
        private static string UserId { get; set; }
        private static async Task CreateAppSettings(ApplicationDbContext context, ILogger logger)
        {
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                try
                {
                    var app = new AppSettings
                    {
                        ApplicationUserId = UserId,
                        FeesPartPayment = false,
                        PTAPartPayment = false,
                        PaymentEvidence = true,
                    };
                    context.AppSettings.Add(app);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating application settings");
                }
            }
        }

        /// <summary>
        /// Seed default roles (Admin, Accountant, Cashier)
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
                        logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                    }
                    else
                    {
                        logger.LogError("Failed to create role '{RoleName}': {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        /// <summary>
        /// Seed example positions
        /// </summary>
        private static async Task SeedPositions(ApplicationDbContext context, ILogger logger)
        {
            if (await context.PositionTables.AnyAsync())
            {
                logger.LogInformation("Positions already exist in database");
                return;
            }

            var positions = new List<PositionTable>
            {
                new PositionTable
                {
                    Name = SD.Positions.PRINCIPAL,
                    Description = "School Principal - Chief administrative officer with full system access",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new PositionTable
                {
                    Name = "Finance Officer",
                    Description = "Finance Officer - Manages financial records and reports",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new PositionTable
                {
                    Name = "Teller",
                    Description = "Teller - Processes payments and generates receipts",
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
            logger.LogInformation("Seeded {Count} positions", positions.Count);
        }

        /// <summary>
        /// Assign roles to positions:
        /// Principal → Admin (full access)
        /// Finance Officer → Accountant
        /// Teller → Cashier
        /// </summary>
        private static async Task AssignRolesToPositions(ApplicationDbContext context, ILogger logger)
        {
            if (await context.PositionRoles.AnyAsync())
            {
                logger.LogInformation("Position-role assignments already exist");
                return;
            }

            var principalPosition = await context.PositionTables.FirstOrDefaultAsync(p => p.Name == SD.Positions.PRINCIPAL);
            var financeOfficerPosition = await context.PositionTables.FirstOrDefaultAsync(p => p.Name == "Finance Officer");
            var tellerPosition = await context.PositionTables.FirstOrDefaultAsync(p => p.Name == "Teller");

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SD.Roles.ADMIN);
            var accountantRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SD.Roles.ACCOUNT);
            var cashierRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == SD.Roles.CASHIER);

            var positionRoles = new List<PositionRole>();

            // Principal gets Admin role (full access)
            if (principalPosition != null && adminRole != null)
            {
                positionRoles.Add(new PositionRole { PositionId = principalPosition.Id, RoleId = adminRole.Id });
            }

            // Finance Officer gets Accountant role
            if (financeOfficerPosition != null && accountantRole != null)
            {
                positionRoles.Add(new PositionRole { PositionId = financeOfficerPosition.Id, RoleId = accountantRole.Id });
            }

            // Teller gets Cashier role
            if (tellerPosition != null && cashierRole != null)
            {
                positionRoles.Add(new PositionRole { PositionId = tellerPosition.Id, RoleId = cashierRole.Id });
            }

            if (positionRoles.Any())
            {
                await context.PositionRoles.AddRangeAsync(positionRoles);
                await context.SaveChangesAsync();
                logger.LogInformation("Assigned roles to {Count} positions", positionRoles.Count);
            }
        }

        /// <summary>
        /// Create admin user as an Employee with Principal position.
        /// Access flows through: Employee → Position (Principal) → Role (Admin) → Permissions (all)
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
                // Ensure admin has an employee record with Principal position
                await EnsureAdminEmployeeRecord(existingAdmin, context, logger);
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
                UserId = adminUser.Id;
                logger.LogInformation("Admin user '{Email}' created successfully", adminEmail);
                logger.LogInformation("Default admin credentials - Email: {Email}, Password: {Password}", adminEmail, adminPassword);

                // Create employee record with Principal position
                await EnsureAdminEmployeeRecord(adminUser, context, logger);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        /// <summary>
        /// Ensure the admin user has an Employee record linked to the Principal position
        /// </summary>
        private static async Task EnsureAdminEmployeeRecord(ApplicationUser adminUser, ApplicationDbContext context, ILogger logger)
        {
            var existingEmployee = await context.Employees
                .FirstOrDefaultAsync(e => e.ApplicationUserId == adminUser.Id);

            var principalPosition = await context.PositionTables
                .FirstOrDefaultAsync(p => p.Name == SD.Positions.PRINCIPAL);

            if (existingEmployee != null)
            {
                // Ensure the existing employee has the Principal position
                if (principalPosition != null && existingEmployee.PositionId != principalPosition.Id)
                {
                    existingEmployee.PositionId = principalPosition.Id;
                    await context.SaveChangesAsync();
                    logger.LogInformation("Updated admin employee to Principal position");
                }
                return;
            }

            // Create employee record for admin
            var employee = new EmployeesTable
            {
                FullName = "System Administrator",
                Phone = "0000000000",
                Address = "School Admin Office",
                Gender = GetEnums.Gender.Male,
                ApplicationUserId = adminUser.Id,
                PositionId = principalPosition?.Id,
                CreatedDate = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            logger.LogInformation("Created employee record for admin user with Principal position");
        }
    }
}
