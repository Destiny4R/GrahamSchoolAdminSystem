using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.roles
{
    [Authorize]
    public class UserRolesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRolesModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogService logService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public UserRoleAssignmentModel RoleAssignment { get; set; } = new UserRoleAssignmentModel();

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
        public Dictionary<string, List<string>> UserRolesMap { get; set; } = new Dictionary<string, List<string>>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load all users
                Users = await _userManager.Users.ToListAsync();

                // Load all roles
                var roles = await _roleManager.Roles.ToListAsync();
                RoleOptions = roles.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                }).ToList();

                // Load user roles mapping
                foreach (var user in Users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    UserRolesMap[user.Id] = userRoles.ToList();
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load users and roles";
                await LogErrorAsync("OnGetAsync", ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAssignRoleAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided";
                return RedirectToPage();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(RoleAssignment.UserId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToPage();
                }

                var role = await _roleManager.FindByIdAsync(RoleAssignment.RoleId);
                if (role == null)
                {
                    TempData["Error"] = "Role not found";
                    return RedirectToPage();
                }

                // Check if user already has this role
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    TempData["Error"] = $"User already has the {role.Name} role";
                    return RedirectToPage();
                }

                // Assign role
                var result = await _userManager.AddToRoleAsync(user, role.Name);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Successfully assigned {role.Name} role to {user.UserName}";
                    await LogUserActionAsync("Assign Role", $"Assigned {role.Name} role to user {user.UserName}");
                }
                else
                {
                    TempData["Error"] = $"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while assigning the role";
                await LogErrorAsync("OnPostAssignRoleAsync", ex.Message);
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    await LogUserActionAsync("Remove Role", $"Removed {roleName} role from user {user.UserName}");
                    return new JsonResult(new { success = true, message = $"Successfully removed {roleName} role" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = $"Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}" });
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("OnPostRemoveRoleAsync", ex.Message);
                return new JsonResult(new { success = false, message = "An error occurred while removing the role" });
            }
        }

        private async Task LogUserActionAsync(string action, string description)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                await _logService.LogUserActionAsync(
                    currentUser.Id,
                    currentUser.UserName ?? currentUser.Email ?? "Unknown",
                    action,
                    "UserRole",
                    currentUser.Id,
                    description,
                    GetClientIpAddress()
                );
            }
        }

        private async Task LogErrorAsync(string action, string errorMessage)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            await _logService.LogErrorAsync(
                action,
                errorMessage,
                null,
                currentUser?.Id ?? "System",
                GetClientIpAddress()
            );
        }

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                   ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                   ?? "Unknown";
        }
    }

    public class UserRoleAssignmentModel
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }
}
