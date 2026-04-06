using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemAccess.Data
{
    // Include ApplicationRole so positions can be linked to roles
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<StudentTable> Students { get; set; }
        public DbSet<TermlyFeesSetup> TermlyFeesSetups { get; set; }
        public DbSet<SchoolClasses> SchoolClasses { get; set; }
        public DbSet<SchoolSubClass> SchoolSubClasses { get; set; }
        public DbSet<SessionYear> SessionYears { get; set; }
        public DbSet<TermRegistration> TermRegistrations { get; set; }
        public DbSet<FeesPaymentTable> FeesPayments { get; set; }
        public DbSet<PTAFeesSetup> PTAFeesSetups { get; set; }
        public DbSet<PTAFeesPayments> PTAFeesPayments { get; set; }
        public DbSet<PositionTable> PositionTables { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<EmployeesTable> Employees { get; set; }
        public DbSet<LogsTable> LogsTables { get; set; }
        public DbSet<OtherPayItemsTable> OtherPayItemsTable { get; set; }
        public DbSet<OtherPayFeesSetUp> OtherPayFeesSetUp { get; set; }
        public DbSet<OtherPayment> OtherPayments { get; set; }

        // Join tables
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        [Obsolete("PositionRole is deprecated. Use UserRole and RolePermission instead.")]
        public DbSet<PositionRole> PositionRoles { get; set; }

        // New roles and permissions system
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite keys for join entities
            builder.Entity<EmployeePosition>()
                .HasKey(ep => new { ep.EmployeeId, ep.PositionId });

            builder.Entity<EmployeePosition>()
                .HasOne(ep => ep.Employee)
                .WithMany(e => e.EmployeePositions)
                .HasForeignKey(ep => ep.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EmployeePosition>()
                .HasOne(ep => ep.Position)
                .WithMany(p => p.EmployeePositions)
                .HasForeignKey(ep => ep.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PositionRole>()
                .HasKey(pr => new { pr.PositionId, pr.RoleId });

            builder.Entity<PositionRole>()
                .HasOne(pr => pr.Position)
                .WithMany(p => p.PositionRoles)
                .HasForeignKey(pr => pr.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PositionRole>()
                .HasOne(pr => pr.Role)
                .WithMany()
                .HasForeignKey(pr => pr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure new roles and permissions system

            // RolePermission composite key
            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserRole composite key
            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional model configuration can go here if needed
        }
    }
}
