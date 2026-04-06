using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.positions
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IndexModel(
            ApplicationDbContext context,
            ILogService logService,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
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
                    .Include(p => p.EmployeePositions)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                Positions = positions.Select(p => new PositionWithStats
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    EmployeeCount = p.EmployeePositions?.Count ?? 0,
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
                    .Include(p => p.EmployeePositions)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (position == null)
                {
                    return new JsonResult(new { success = false, message = "Position not found" });
                }

                // Check if position is assigned to any employees
                if (position.EmployeePositions != null && position.EmployeePositions.Any())
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = $"Cannot delete position '{position.Name}' because it is assigned to {position.EmployeePositions.Count} employee(s)"
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
}
