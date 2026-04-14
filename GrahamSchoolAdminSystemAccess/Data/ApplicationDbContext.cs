using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace GrahamSchoolAdminSystemAccess.Data
{
    // Include ApplicationRole so positions can be linked to roles
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<StudentTable> Students { get; set; }
        public DbSet<SchoolClasses> SchoolClasses { get; set; }
        public DbSet<SchoolSubClass> SchoolSubClasses { get; set; }
        public DbSet<SessionYear> SessionYears { get; set; }
        public DbSet<TermRegistration> TermRegistrations { get; set; }
        public DbSet<PositionTable> PositionTables { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<EmployeesTable> Employees { get; set; }
        public DbSet<LogsTable> LogsTables { get; set; }

        // Payment system tables
        public DbSet<PaymentCategory> PaymentCategories { get; set; }
        public DbSet<PaymentItem> PaymentItems { get; set; }
        public DbSet<PaymentSetup> PaymentSetups { get; set; }
        public DbSet<StudentPayment> StudentPayments { get; set; }
        public DbSet<StudentPaymentItem> StudentPaymentItems { get; set; }

        // Position-based authorization tables
        public DbSet<PositionRole> PositionRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Employee → Position (single FK)
            builder.Entity<EmployeesTable>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            // PositionRole composite key (Position ↔ Role many-to-many)
            builder.Entity<PositionRole>()
                .HasKey(pr => new { pr.PositionId, pr.RoleId });

            builder.Entity<PositionRole>()
                .HasOne(pr => pr.Position)
                .WithMany(p => p.PositionRoles)
                .HasForeignKey(pr => pr.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PositionRole>()
                .HasOne(pr => pr.Role)
                .WithMany(r => r.PositionRoles)
                .HasForeignKey(pr => pr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // RolePermission composite key (Role ↔ Permission many-to-many)
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
            
            builder.Entity<AppSettings>()
               .HasIndex(a => a.ApplicationUserId)
               .IsUnique();

            // PaymentSetup: unique constraint on (PaymentItemId + SessionId + Term + ClassId)
            builder.Entity<PaymentSetup>()
                .HasIndex(ps => new { ps.PaymentItemId, ps.SessionId, ps.Term, ps.ClassId })
                .IsUnique();

            // PaymentItem → PaymentCategory
            builder.Entity<PaymentItem>()
                .HasOne(pi => pi.PaymentCategory)
                .WithMany(pc => pc.PaymentItems)
                .HasForeignKey(pi => pi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentSetup → PaymentItem
            builder.Entity<PaymentSetup>()
                .HasOne(ps => ps.PaymentItem)
                .WithMany(pi => pi.PaymentSetups)
                .HasForeignKey(ps => ps.PaymentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentPayment → TermRegistration
            builder.Entity<StudentPayment>()
                .HasOne(sp => sp.TermRegistration)
                .WithMany()
                .HasForeignKey(sp => sp.TermRegId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentPaymentItem → StudentPayment
            builder.Entity<StudentPaymentItem>()
                .HasOne(spi => spi.StudentPayment)
                .WithMany(sp => sp.PaymentItems)
                .HasForeignKey(spi => spi.StudentPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentPaymentItem → PaymentItem
            builder.Entity<StudentPaymentItem>()
                .HasOne(spi => spi.PaymentItem)
                .WithMany()
                .HasForeignKey(spi => spi.PaymentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
