using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class UsersServices : IUsersServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersServices> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersServices(ApplicationDbContext context, ILogger<UsersServices> logger, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ILogService logService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from HttpContext
        /// </summary>
        private string GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        /// <summary>
        /// Get current user name from HttpContext
        /// </summary>
        private string GetCurrentUserName()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }

        /// <summary>
        /// Get client IP address from HttpContext
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext == null)
                    return "Unknown";

                if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                    return httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

                return httpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion

        #region Position Management

        public async Task<(bool Succeeded, string Message)> CreatePositionAsync(PositionViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();

                if (await _context.PositionTables.AnyAsync(x => x.Name == name))
                    return (false, SD.Messages.ERROR_POSITION_EXISTS);

                var position = new PositionTable
                {
                    Name = name,
                    Description = model.Description?.Trim(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.PositionTables.Add(position);
                await _context.SaveChangesAsync();

                return (true, SD.Messages.SUCCESS_POSITION_CREATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating position");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        public async Task<(bool Succeeded, string Message)> UpdatePositionAsync(PositionViewModel model)
        {
            try
            {
                if (!model.Id.HasValue || model.Id <= 0)
                    return (false, SD.Messages.ERROR_INVALID_POSITION);

                var position = await _context.PositionTables.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (position == null)
                    return (false, SD.Messages.ERROR_POSITION_NOT_FOUND);

                // Protect Principal position from modification
                if (position.Name == SD.Positions.PRINCIPAL)
                    return (false, SD.Messages.ERROR_PRINCIPAL_PROTECTED);
                    return (false, SD.Messages.ERROR_POSITION_NOT_FOUND);

                var name = model.Name?.Trim();
                if (await _context.PositionTables.AnyAsync(x => x.Name == name && x.Id != model.Id))
                    return (false, SD.Messages.ERROR_POSITION_EXISTS);

                position.Name = name;
                position.Description = model.Description?.Trim();
                position.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return (true, SD.Messages.SUCCESS_POSITION_UPDATED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating position");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        public async Task<(bool Succeeded, string Message)> DeletePositionAsync(int positionId)
        {
            try
            {
                if (positionId <= 0)
                    return (false, SD.Messages.ERROR_INVALID_POSITION);

                // Protect Principal position from deletion
                var positionToCheck = await _context.PositionTables.FirstOrDefaultAsync(x => x.Id == positionId);
                if (positionToCheck != null && positionToCheck.Name == SD.Positions.PRINCIPAL)
                    return (false, SD.Messages.ERROR_PRINCIPAL_PROTECTED);

                // Check if position is assigned to any employees
                if (await _context.Employees.AnyAsync(x => x.PositionId == positionId))
                    return (false, SD.Messages.ERROR_POSITION_IN_USE);

                var position = await _context.PositionTables.FirstOrDefaultAsync(x => x.Id == positionId);
                if (position == null)
                    return (false, SD.Messages.ERROR_POSITION_NOT_FOUND);

                _context.PositionTables.Remove(position);
                await _context.SaveChangesAsync();

                return (true, SD.Messages.SUCCESS_POSITION_DELETED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting position");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        public async Task<PositionDto> GetPositionByIdAsync(int positionId)
        {
            try
            {
                var position = await _context.PositionTables
                    .Include(p => p.PositionRoles)
                    .ThenInclude(pr => pr.Role)
                    .Include(p => p.Employees)
                    .FirstOrDefaultAsync(x => x.Id == positionId);

                if (position == null)
                    return null;

                return MapToPositionDto(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position by id");
                return null;
            }
        }

        public async Task<(List<PositionDto> data, int recordsTotal, int recordsFiltered)> GetPositionsAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection)
        {
            try
            {
                var query = _context.PositionTables
                    .Include(p => p.PositionRoles)
                    .ThenInclude(pr => pr.Role)
                    .Include(p => p.Employees)
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchValue))
                    query = query.Where(x => x.Name.Contains(searchValue) || x.Description.Contains(searchValue));

                var recordsFiltered = await query.CountAsync();

                // Sorting
                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                    2 => sortDirection == "asc" ? query.OrderBy(x => x.CreatedDate) : query.OrderByDescending(x => x.CreatedDate),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var positions = await query.Skip(start).Take(length).ToListAsync();
                var data = positions.Select(MapToPositionDto).ToList();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions");
                return (new List<PositionDto>(), 0, 0);
            }
        }

        #endregion

        #region Position-Role Assignment

        public async Task<(bool Succeeded, string Message)> AssignRolesToPositionAsync(int positionId, List<string> roleIds)
        {
            try
            {
                if (positionId <= 0 || roleIds == null || roleIds.Count == 0)
                    return (false, SD.Messages.ERROR_INVALID_POSITION);

                var position = await _context.PositionTables.FirstOrDefaultAsync(x => x.Id == positionId);
                if (position == null)
                    return (false, SD.Messages.ERROR_POSITION_NOT_FOUND);

                // Protect Principal position — role assignments cannot be changed
                if (position.Name == SD.Positions.PRINCIPAL)
                    return (false, SD.Messages.ERROR_PRINCIPAL_PROTECTED);

                // Remove existing assignments
                var existingRoles = await _context.PositionRoles.Where(x => x.PositionId == positionId).ToListAsync();
                _context.PositionRoles.RemoveRange(existingRoles);

                // Add new assignments
                foreach (var roleId in roleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        var positionRole = new PositionRole { PositionId = positionId, RoleId = roleId };
                        _context.PositionRoles.Add(positionRole);
                    }
                }

                await _context.SaveChangesAsync();
                return (true, SD.Messages.SUCCESS_ROLES_ASSIGNED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to position");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        public async Task<AssignRoleViewModel> GetRoleAssignmentViewAsync(int positionId)
        {
            try
            {
                var position = await _context.PositionTables
                    .Include(p => p.PositionRoles)
                    .FirstOrDefaultAsync(x => x.Id == positionId);

                if (position == null)
                    return null;

                var allRoles = await _roleManager.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .ToListAsync();

                var allPermissions = await _context.Permissions
                    .OrderBy(p => p.Name)
                    .Select(p => new PermissionViewModel { Id = p.Id, Name = p.Name })
                    .ToListAsync();

                var assignedRoleIds = position.PositionRoles.Select(pr => pr.RoleId).ToList();

                var availableRoles = allRoles.Select(r => new RoleCheckboxViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsAssigned = assignedRoleIds.Contains(r.Id),
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList(),
                    PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
                }).ToList();

                return new AssignRoleViewModel
                {
                    PositionId = positionId,
                    PositionName = position.Name,
                    AvailableRoles = availableRoles,
                    AssignedRoles = allRoles.Where(r => assignedRoleIds.Contains(r.Id)).Select(r => r.Name).ToList(),
                    SelectedRoleIds = assignedRoleIds,
                    AllPermissions = allPermissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role assignment view");
                return null;
            }
        }

        public IEnumerable<SelectListItem> PositionsList()
        {
            var positions = _context.PositionTables
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    }).ToList();
            return positions;
        }

        public async Task<(bool Succeeded, string Message)> RemoveRoleFromPositionAsync(int positionId, string roleId)
        {
            try
            {
                if (positionId <= 0 || string.IsNullOrWhiteSpace(roleId))
                    return (false, SD.Messages.ERROR_INVALID_POSITION);

                // Protect Principal position — roles cannot be removed
                var position = await _context.PositionTables.FirstOrDefaultAsync(x => x.Id == positionId);
                if (position != null && position.Name == SD.Positions.PRINCIPAL)
                    return (false, SD.Messages.ERROR_PRINCIPAL_PROTECTED);

                var positionRole = await _context.PositionRoles
                    .FirstOrDefaultAsync(x => x.PositionId == positionId && x.RoleId == roleId);

                if (positionRole == null)
                    return (false, "Role assignment not found");

                _context.PositionRoles.Remove(positionRole);
                await _context.SaveChangesAsync();

                return (true, "Role removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from position");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        #endregion

        #region Role-Permission Management

        public async Task<(bool Succeeded, string Message)> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                    return (false, "Invalid role selected");

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return (false, "Role not found");

                // Remove existing permissions for this role
                var existingPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();
                _context.RolePermissions.RemoveRange(existingPermissions);

                // Add new permissions
                if (permissionIds != null && permissionIds.Count > 0)
                {
                    foreach (var permissionId in permissionIds)
                    {
                        var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);
                        if (permissionExists)
                        {
                            _context.RolePermissions.Add(new RolePermission
                            {
                                RoleId = roleId,
                                PermissionId = permissionId,
                                AssignedDate = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return (true, $"Permissions updated for role '{role.Name}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permissions");
                return (false, SD.Messages.ERROR_UNEXPECTED);
            }
        }

        #endregion

        #region Get Available Roles

        public async Task<List<RolePermissionDto>> GetAvailableRolesWithPermissionsAsync()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .ToListAsync();

                return roles.Select(r => new RolePermissionDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available roles");
                return new List<RolePermissionDto>();
            }
        }

        #endregion

        #region Employee Management

        public async Task<(List<EmployeeViewModel> data, int recordsTotal, int recordsFiltered)> GetEmployeesAsync(int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.Employees
                    .Include(e => e.ApplicationUser)
                    .Include(e => e.Position)
                    .ThenInclude(p => p.PositionRoles)
                    .ThenInclude(pr => pr.Role)
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(x => 
                        x.FullName.Contains(searchValue) || 
                        x.Phone.Contains(searchValue) || 
                        x.ApplicationUser.Email.Contains(searchValue) ||
                        x.Address.Contains(searchValue));
                }

                var recordsFiltered = await query.CountAsync();

                // Sorting
                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
                    2 => sortDirection == "asc" ? query.OrderBy(x => x.Phone) : query.OrderByDescending(x => x.Phone),
                    _ => query.OrderBy(x => x.FullName)
                };

                var employees = await query.Skip(start).Take(length).ToListAsync();
                var data = employees.Select(MapToEmployeeViewModel).ToList();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees");
                return (new List<EmployeeViewModel>(), 0, 0);
            }
        }

        public async Task<EmployeeViewModel> GetEmployeeByIdAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.ApplicationUser)
                    .Include(e => e.Position)
                    .ThenInclude(p => p.PositionRoles)
                    .ThenInclude(pr => pr.Role)
                    .FirstOrDefaultAsync(x => x.Id == employeeId);

                if (employee == null)
                    return null;

                return MapToEmployeeViewModel(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting employee by id {employeeId}");
                return null;
            }
        }

        public async Task<(bool Succeeded, string Message, object Data)> CreateEmployeeAsync(EmployeeViewModel model)
        {
            try
            {
                // Check if a user with the same email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return (false, "A user account with this email already exists", null);

                // Validate position if provided
                if (model.PositionId.HasValue && model.PositionId > 0)
                {
                    var positionExists = await _context.PositionTables.AnyAsync(p => p.Id == model.PositionId);
                    if (!positionExists)
                        return (false, "Selected position does not exist", null);
                }

                // Create the ApplicationUser (login account) with default password
                var applicationUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(applicationUser, "12345678");
                if (!identityResult.Succeeded)
                {
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to create user account for {Email}: {Errors}", model.Email, errors);
                    return (false, $"Failed to create user account: {errors}", null);
                }

                // Create the employee record linked to the new user
                var employee = new EmployeesTable
                {
                    FullName = $"{model.FirstName} {model.LastName}",
                    Phone = model.Phone,
                    Address = model.Address ?? string.Empty,
                    Gender = (GetEnums.Gender)(model.GenderId ?? 0),
                    ApplicationUserId = applicationUser.Id,
                    PositionId = model.PositionId > 0 ? model.PositionId : null
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return (true, "Employee created successfully with default login (password: 12345678)", employee.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return (false, "Error creating employee", null);
            }
        }

        public async Task<(bool Succeeded, string Message)> UpdateEmployeeAsync(EmployeeViewModel model)
        {
            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (employee == null)
                    return (false, "Employee not found");

                // Check if email is already used by another employee
                var emailExists = await _context.Employees
                    .Include(e => e.ApplicationUser)
                    .AnyAsync(x => x.Id != model.Id && 
                        x.ApplicationUser.Email == model.Email);

                if (emailExists)
                    return (false, "Email is already in use");

                // Validate position if provided
                if (model.PositionId.HasValue && model.PositionId > 0)
                {
                    var positionExists = await _context.PositionTables.AnyAsync(p => p.Id == model.PositionId);
                    if (!positionExists)
                        return (false, "Selected position does not exist");
                }

                // Protect admin user — cannot change position away from Principal
                var currentPosition = await _context.PositionTables
                    .FirstOrDefaultAsync(p => p.Id == employee.PositionId);
                if (currentPosition != null && currentPosition.Name == SD.Positions.PRINCIPAL)
                {
                    var principalPosition = await _context.PositionTables
                        .FirstOrDefaultAsync(p => p.Name == SD.Positions.PRINCIPAL);
                    if (principalPosition != null && model.PositionId != principalPosition.Id)
                        return (false, SD.Messages.ERROR_ADMIN_POSITION_PROTECTED);
                }

                employee.FullName = $"{model.FirstName} {model.LastName}";
                employee.Phone = model.Phone;
                employee.Address = model.Address ?? string.Empty;
                employee.PositionId = model.PositionId > 0 ? model.PositionId : null;

                if (model.GenderId.HasValue)
                    employee.Gender = (GetEnums.Gender)model.GenderId.Value;

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return (true, "Employee updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee {model.Id}");
                return (false, "Error updating employee");
            }
        }

        public async Task<(bool Succeeded, string Message)> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(x => x.Id == employeeId);

                if (employee == null)
                    return (false, "Employee not found");

                var applicationUserId = employee.ApplicationUserId;

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                // Delete the associated user account
                if (!string.IsNullOrWhiteSpace(applicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(applicationUserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                            _logger.LogWarning("Employee deleted but failed to remove user account {UserId}", applicationUserId);
                    }
                }

                return (true, "Employee and user account deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting employee {employeeId}");
                return (false, "Error deleting employee");
            }
        }

        #endregion

        #region Password Management

        public async Task<(bool Succeeded, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return (false, "Invalid user");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return (false, "User not found");

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, errors);
                    return (false, errors);
                }

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return (false, "An unexpected error occurred while changing the password");
            }
        }

        #endregion

        #region Helper Methods

        private EmployeeViewModel MapToEmployeeViewModel(EmployeesTable employee)
        {
            return new EmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
                LastName = string.Join(" ", employee.FullName?.Split(' ').Skip(1) ?? new[] { string.Empty }),
                Email = employee.ApplicationUser?.Email ?? string.Empty,
                Phone = employee.Phone,
                Address = employee.Address,
                GenderId = (int?)employee.Gender,
                ApplicationUserId = employee.ApplicationUserId,
                IsActive = employee.ApplicationUser?.LockoutEnd == null,
                PositionId = employee.PositionId,
                PositionName = employee.Position?.Name ?? string.Empty,
                Roles = employee.Position?.PositionRoles?
                    .Select(pr => pr.Role?.Name ?? string.Empty)
                    .Where(r => !string.IsNullOrEmpty(r))
                    .ToList() ?? new List<string>()
            };
        }

        private PositionDto MapToPositionDto(PositionTable position)
        {
            return new PositionDto
            {
                Id = position.Id,
                Name = position.Name,
                Description = position.Description,
                EmployeeCount = position.Employees?.Count ?? 0,
                AssignedRoles = position.PositionRoles?.Select(pr => pr.Role?.Name).Where(n => n != null).ToList() ?? new List<string>(),
                CreatedDate = position.CreatedDate,
                UpdatedDate = position.UpdatedDate
            };
        }

        #endregion
        #region AppSettings
        //create, update and get
        public async Task<(bool Succeeded, string Message)> CreateOrUpdateAppSettingsAsync(AppSettingViewModel model)
        {
                var userId = GetCurrentUserId();
                var userName = GetCurrentUserName();
            try
            {

                var existingSettings = await _context.AppSettings.FirstOrDefaultAsync();
                if (existingSettings != null)
                {
                    existingSettings.FeesPartPayment = model.feespart;
                    existingSettings.PTAPartPayment = model.ptapart;
                    existingSettings.PaymentEvidence = model.PaymentEvidence;
                    existingSettings.Term = (GetEnums.Term)model.term;
                    existingSettings.SessionId = model.sessionId;
                    _context.AppSettings.Update(existingSettings);
                    await _context.SaveChangesAsync();

                    await _logService.LogUserActionAsync(
                        userId: userId,
                        userName: userName,
                        action: "User Updated App Settings",
                        entityType: "AppSettings",
                        entityId: "0",
                        message: "Updated app settings",
                        ipAddress: GetClientIpAddress(),
                        details: $"User with ID {userId} updated app settings"
                    );

                    return (true, "App settings updated successfully");
                    //Log the update action in service layer
                }
                else
                {
                    var newSettings = new AppSettings
                    {
                        FeesPartPayment = model.feespart,
                        PTAPartPayment = model.ptapart,
                        PaymentEvidence = model.PaymentEvidence,
                        Term = (GetEnums.Term)model.term,
                        SessionId = model.sessionId,
                        ApplicationUserId = userId
                    };
                    _context.AppSettings.Add(newSettings);
                    await _context.SaveChangesAsync();

                    await _logService.LogUserActionAsync(
                        userId: userId,
                        userName: userName,
                        action: "User Created App Settings",
                        entityType: "AppSettings",
                        entityId: "0",
                        message: "Created app settings",
                        ipAddress: GetClientIpAddress(),
                        details: $"User with ID {userId} created app settings"
                    );

                    return (true, "App settings created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating app settings for user {UserId}", userId);
                return (false, "An unexpected error occurred while saving app settings");
            }
        }
        public async Task<AppSettingViewModel> GetAppSettingsByUserIdAsync()
        {
            try
            {
                var settings = await _context.AppSettings.FirstOrDefaultAsync(); 
                await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "View App Settings",
                        entityType: "AppSettings",
                        entityId: "0",
                        message: "Viewed app settings",
                        ipAddress: GetClientIpAddress(),
                        details: $"User with ID {GetCurrentUserId()} viewed app settings"
                    );
                if (settings == null)
                    return new AppSettingViewModel { feespart = false, ptapart = false, PaymentEvidence = false, term = 0, sessionId = 0 };
                return new AppSettingViewModel
                {
                    id = settings.Id,
                    feespart = settings.FeesPartPayment,
                    ptapart = settings.PTAPartPayment,
                    PaymentEvidence = settings.PaymentEvidence,
                    term = (int)settings.Term,
                    sessionId = settings.SessionId
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving app settings for user {UserId}");
                return new AppSettingViewModel { feespart = false, ptapart = false, PaymentEvidence = false, term = 0, sessionId = 0 };
            }
        }
        #endregion
    }
}

