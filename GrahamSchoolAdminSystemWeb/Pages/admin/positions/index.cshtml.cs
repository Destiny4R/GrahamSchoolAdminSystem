using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.positions
{
    [Authorize]
    [RequireRole(SD.Roles.ADMIN)]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsersServices _usersServices;

        public IndexModel(
            ApplicationDbContext context,
            ILogService logService,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IUsersServices usersServices)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _usersServices = usersServices;
        }

        [BindProperty]
        public PositionInputModel Position { get; set; } = new PositionInputModel();

        public List<PositionWithStats> Positions { get; set; } = new List<PositionWithStats>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load all positions with employee count
                var positions = await _context.PositionTables
                    .Include(p => p.Employees)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                Positions = positions.Select(p => new PositionWithStats
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    EmployeeCount = p.Employees?.Count ?? 0,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate
                }).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load positions";
                await LogErrorAsync("OnGetAsync", ex.Message);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data provided";
                return RedirectToPage();
            }

            try
            {
                if (Position.Id > 0)
                {
                    // Update existing position
                    var existingPosition = await _context.PositionTables.FindAsync(Position.Id);
                    if (existingPosition == null)
                    {
                        TempData["Error"] = "Position not found";
                        return RedirectToPage();
                    }

                    // Protect Principal position from modification
                    if (existingPosition.Name == SD.Positions.PRINCIPAL)
                    {
                        TempData["Error"] = SD.Messages.ERROR_PRINCIPAL_PROTECTED;
                        return RedirectToPage();
                    }

                    existingPosition.Name = Position.Name;
                    existingPosition.Description = Position.Description;
                    existingPosition.UpdatedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Position '{Position.Name}' updated successfully";
                    await LogUserActionAsync("Update Position", $"Updated position: {Position.Name}");
                }
                else
                {
                    // Create new position
                    var newPosition = new PositionTable
                    {
                        Name = Position.Name,
                        Description = Position.Description,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };

                    _context.PositionTables.Add(newPosition);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Position '{Position.Name}' created successfully";
                    await LogUserActionAsync("Create Position", $"Created new position: {Position.Name}");
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the position";
                await LogErrorAsync("OnPostAsync", ex.Message);
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var position = await _context.PositionTables
                    .Include(p => p.Employees)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (position == null)
                {
                    return new JsonResult(new { success = false, message = "Position not found" });
                }

                // Protect Principal position from deletion
                if (position.Name == SD.Positions.PRINCIPAL)
                {
                    return new JsonResult(new { success = false, message = SD.Messages.ERROR_PRINCIPAL_PROTECTED });
                }

                // Check if position is assigned to any employees
                if (position.Employees != null && position.Employees.Any())
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Cannot delete position '{position.Name}' because it is assigned to {position.Employees.Count} employee(s)"
                    });
                }

                var positionName = position.Name;
                _context.PositionTables.Remove(position);
                await _context.SaveChangesAsync();

                await LogUserActionAsync("Delete Position", $"Deleted position: {positionName}");

                return new JsonResult(new
                {
                    success = true,
                    message = $"Position '{positionName}' deleted successfully"
                });
            }
            catch (Exception ex)
            {
                await LogErrorAsync("OnPostDeleteAsync", ex.Message);
                return new JsonResult(new
                {
                    success = false,
                    message = "An error occurred while deleting the position"
                });
            }
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            try
            {
                var position = await _context.PositionTables.FindAsync(id);
                if (position == null)
                {
                    return new JsonResult(new { success = false, message = "Position not found" });
                }

                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        id = position.Id,
                        name = position.Name,
                        description = position.Description
                    }
                });
            }
            catch (Exception ex)
            {
                await LogErrorAsync("OnGetEditAsync", ex.Message);
                return new JsonResult(new { success = false, message = "Error loading position data" });
            }
        }

        public async Task<IActionResult> OnGetRoleAssignmentAsync(int id)
        {
            try
            {
                var viewModel = await _usersServices.GetRoleAssignmentViewAsync(id);
                if (viewModel == null)
                {
                    return new JsonResult(new { success = false, message = "Position not found" });
                }

                return new JsonResult(new
                {
                    success = true,
                    data = new
                    {
                        positionId = viewModel.PositionId,
                        positionName = viewModel.PositionName,
                        allPermissions = viewModel.AllPermissions.Select(p => new
                        {
                            id = p.Id,
                            name = p.Name
                        }),
                        availableRoles = viewModel.AvailableRoles.Select(r => new
                        {
                            roleId = r.RoleId,
                            roleName = r.RoleName,
                            isAssigned = r.IsAssigned,
                            permissions = r.Permissions,
                            permissionIds = r.PermissionIds
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                await LogErrorAsync("OnGetRoleAssignmentAsync", ex.Message);
                return new JsonResult(new { success = false, message = "Error loading role assignment data" });
            }
        }

        public async Task<IActionResult> OnPostAssignRolesAsync([FromBody] AssignRolesRequest request)
        {
            try
            {
                if (request == null || request.PositionId <= 0)
                {
                    return new JsonResult(new { success = false, message = "Invalid request data" });
                }

                // Protect Principal position — roles and permissions cannot be modified
                var position = await _context.PositionTables.FirstOrDefaultAsync(p => p.Id == request.PositionId);
                if (position != null && position.Name == SD.Positions.PRINCIPAL)
                {
                    return new JsonResult(new { success = false, message = SD.Messages.ERROR_PRINCIPAL_PROTECTED });
                }

                // 1. Update role-permission assignments for each role
                if (request.RolePermissions != null)
                {
                    foreach (var rp in request.RolePermissions)
                    {
                        var result = await _usersServices.UpdateRolePermissionsAsync(rp.RoleId, rp.PermissionIds ?? new List<int>());
                        if (!result.Succeeded)
                        {
                            return new JsonResult(new { success = false, message = result.Message });
                        }
                    }
                }

                // 2. Update position-role assignments
                var roleIds = request.SelectedRoleIds ?? new List<string>();

                if (roleIds.Count == 0)
                {
                    var existingRoles = await _context.PositionRoles
                        .Where(x => x.PositionId == request.PositionId)
                        .ToListAsync();
                    _context.PositionRoles.RemoveRange(existingRoles);
                    await _context.SaveChangesAsync();

                    await LogUserActionAsync("Assign Roles & Permissions", $"Removed all roles from position ID: {request.PositionId}");
                    return new JsonResult(new { success = true, message = "Roles and permissions updated successfully" });
                }

                var assignResult = await _usersServices.AssignRolesToPositionAsync(request.PositionId, roleIds);

                if (assignResult.Succeeded)
                {
                    await LogUserActionAsync("Assign Roles & Permissions", $"Updated roles and permissions for position ID: {request.PositionId}");
                }

                return new JsonResult(new { success = assignResult.Succeeded, message = "Roles and permissions updated successfully" });
            }
            catch (Exception ex)
            {
                await LogErrorAsync("OnPostAssignRolesAsync", ex.Message);
                return new JsonResult(new { success = false, message = "An error occurred while saving" });
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
                    "Position",
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

    public class PositionInputModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PositionWithStats
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class AssignRolesRequest
    {
        public int PositionId { get; set; }
        public List<string> SelectedRoleIds { get; set; } = new List<string>();
        public List<RolePermissionUpdate> RolePermissions { get; set; } = new List<RolePermissionUpdate>();
    }

    public class RolePermissionUpdate
    {
        public string RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
