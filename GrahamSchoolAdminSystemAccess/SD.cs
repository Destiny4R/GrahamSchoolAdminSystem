using Microsoft.AspNetCore.Http;
using System;
using SixLabors.ImageSharp;

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

        // Protected Position Names
        public static class Positions
        {
            public const string PRINCIPAL = "Principal";
        }
        public static string ToNaira(decimal money)
        {
            char naira = (char)8358;
            string Money;
            Money = money.ToString("c");
            return Money.Replace('$', naira);
        }
        // Permission Names
        public static class Permissions
        {
            public const string CREATE = "Create";
            public const string EDIT = "Edit";
            public const string DELETE = "Delete";
            public const string VIEW = "View";
            public const string REPORT = "Report";
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
            public const string ERROR_PRINCIPAL_PROTECTED = "The Principal position is a system-protected position and cannot be modified or deleted";
            public const string ERROR_ADMIN_ROLE_PROTECTED = "The Admin role permissions cannot be modified when assigned to the Principal position";
            public const string ERROR_ADMIN_POSITION_PROTECTED = "The admin user's position cannot be changed from Principal";
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
            Permissions.VIEW,
            Permissions.REPORT
        };

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

        public static int? ParseGenderId(string genderValue)
        {
            if (string.IsNullOrWhiteSpace(genderValue))
                return null;

            genderValue = genderValue.Trim().ToLower();

            return genderValue switch
            {
                "male" or "m" or "1" => 1,
                "female" or "f" or "2" => 2,
                _ => null
            };
        }

        public static bool IsExcelFileSecure(IFormFile file)
        {
            if (file == null || file.Length < 4)
                return false;

            using var stream = file.OpenReadStream();
            using var reader = new BinaryReader(stream);

            var headerBytes = reader.ReadBytes(4);

            // XLS (OLE Compound File): D0 CF 11 E0
            var xlsSignature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 };

            // XLSX (ZIP format): 50 4B 03 04
            var xlsxSignature = new byte[] { 0x50, 0x4B, 0x03, 0x04 };

            return headerBytes.SequenceEqual(xlsSignature) ||
                   headerBytes.SequenceEqual(xlsxSignature);
        }

        public static bool IsExcelFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Allowed extensions
            var allowedExtensions = new[] { ".xls", ".xlsx" };

            // Allowed MIME types
            var allowedMimeTypes = new[]
            {
            "application/vnd.ms-excel", // .xls
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" // .xlsx
        };

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                return false;

            if (!allowedMimeTypes.Contains(file.ContentType))
                return false;

            return true;
        }
    }
}
