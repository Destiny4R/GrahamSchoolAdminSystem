using Microsoft.AspNetCore.Http;
using System;
using SixLabors.ImageSharp;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemAccess
{
    public static class SD
    {
        // Role Names
        public static class Roles
        {
            public const string ADMIN = "Admin";
            public const string ACCOUNT = "Accountant";
            public const string CASHIER = "Cashier";
        }

        // Permission Names
        public static class Permissions
        {
            public const string CREATE = "Create";
            public const string EDIT = "Edit";
            public const string DELETE = "Delete";
            public const string VIEW = "View";
        }

        // Route Names
        public static class Routes
        {
            public const string POSITIONS_INDEX = "/admin/positions/index";
            public const string POSITIONS_ADD = "/admin/positions/add";
            public const string POSITIONS_EDIT = "/admin/positions/edit";
        }

        // Message Templates
        public static class Messages
        {
            public const string SUCCESS_POSITION_CREATED = "Position created successfully";
            public const string SUCCESS_POSITION_UPDATED = "Position updated successfully";
            public const string SUCCESS_POSITION_DELETED = "Position deleted successfully";
            public const string SUCCESS_ROLES_ASSIGNED = "Roles assigned successfully";
            public const string ERROR_POSITION_EXISTS = "Position already exists";
            public const string ERROR_POSITION_NOT_FOUND = "Position not found";
            public const string ERROR_POSITION_IN_USE = "Position is assigned to employees and cannot be deleted";
            public const string ERROR_ROLE_ALREADY_ASSIGNED = "Role is already assigned to this position";
            public const string ERROR_INVALID_POSITION = "Invalid position selected";
            public const string ERROR_UNEXPECTED = "An unexpected error occurred. Please try again.";
        }

        // Default Roles to Initialize
        public static List<string> GetDefaultRoles() => new()
        {
            Roles.ADMIN,
            Roles.ACCOUNT,
            Roles.CASHIER
        };

        // Get all permission names
        public static List<string> GetAllPermissions() => new()
        {
            Permissions.CREATE,
            Permissions.EDIT,
            Permissions.DELETE,
            Permissions.VIEW
        };

        // Permission mapping for roles (legacy - for reference only)
        public static Dictionary<string, List<string>> GetRolePermissions()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    Roles.ADMIN, new List<string>
                    {
                        "View Dashboard",
                        "Manage Positions",
                        "Manage Employees",
                        "Manage Roles",
                        "View Reports",
                        "Manage Students",
                        "Manage Fees",
                        "Manage Session",
                        "Manage Classes",
                        "User Management",
                        "System Settings"
                    }
                },
                {
                    Roles.ACCOUNT, new List<string>
                    {
                        "View Dashboard",
                        "View Reports",
                        "Manage Fees",
                        "Manage Session",
                        "View Students",
                        "View Classes"
                    }
                },
                {
                    Roles.CASHIER, new List<string>
                    {
                        "View Dashboard",
                        "Process Payments",
                        "View Student Fees",
                        "Generate Receipts",
                        "View Reports"
                    }
                }
            };
        }

        /// <summary>
        /// Get permission color badge style
        /// </summary>
        public static string GetPermissionBadgeClass(string permission) => "badge bg-info";

        /// <summary>
        /// Get role badge color
        /// </summary>
        public static string GetRoleBadgeClass(string roleName) => roleName switch
        {
            Roles.ADMIN => "badge bg-danger",
            Roles.ACCOUNT => "badge bg-warning text-dark",
            Roles.CASHIER => "badge bg-success",
            _ => "badge bg-secondary"
        };

        public static string GenerateUniqueNumber()
        {
            string dateTimeString = DateTime.Now.ToString("yyyyMMddHHmm");
            Random random = new Random();
            string randomAlphabets = new string(Enumerable.Range(0, 3).Select(_ => (char)random.Next('A', 'Z' + 1)).ToArray());
            return dateTimeString + randomAlphabets;
        }

        private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };
        public static readonly string[] AllowedExtensionsFiles =  { ".jpg", ".jpeg", ".png", ".pneg", ".pdf", ".gif" };
        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // 1️⃣ Extension check
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                return false;

            // 2️⃣ Content check (real image)
            try
            {
                using var stream = file.OpenReadStream();
                stream.Position = 0;

                using var image = Image.Load(stream);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //Write a method for generating Payment invoice numbers
        public static string GetSpanBadgeStatus(PaymentStatus status)
        {
            switch (status)
            {
                case PaymentStatus.Approved:
                    return "<Span class='badge bg-success'><i class='bi bi-check2-all me-2'></i>Approved</span>";
                case PaymentStatus.Pending:
                    return "<Span class='badge bg-secondary'><i class='bi bi-arrow-clockwise me-2'></i>Pending</span>";
                default:
                    return "<Span class='badge bg-danger'><i class='bi bi-x me-2'></i>Rejected</span>";
            }
        }

        public static string GetSpanBadgeState(PaymentState state)
        {
            switch (state)
            {
                case PaymentState.Cancelled:

                    return "<Span class='badge bg-warning'><i class='bi bi-x me-2'></i>Cancelled</span>";
                case PaymentState.PartPayment:

                    return "<Span class='badge bg-info'><i class='bi bi-circle-half me-2'></i>Part Payment</span>";
                default:
                    return "<Span class='badge bg-success'><i class='bi bi-check2-all me-2'></i>Completed</span>";
            }
        }

        public static string ToNaira(decimal money)
        {
            char naira = (char)8358;
            string Money;
            Money = money.ToString("c");
            return Money.Replace('$', naira);
        }
    }
}
