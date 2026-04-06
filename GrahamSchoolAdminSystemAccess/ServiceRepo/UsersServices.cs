using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UsersServices(ApplicationDbContext context, ILogger<UsersServices> logger, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

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

                // Check if position is assigned to employees
                if (await _context.EmployeePositions.AnyAsync(x => x.PositionId == positionId))
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
                    .Include(p => p.EmployeePositions)
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
                    .Include(p => p.EmployeePositions)
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

                var allRoles = await _roleManager.Roles.ToListAsync();
                var assignedRoleIds = position.PositionRoles.Select(pr => pr.RoleId).ToList();

                var availableRoles = allRoles.Select(r => new RoleCheckboxViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsAssigned = assignedRoleIds.Contains(r.Id),
                    Permissions = SD.GetRolePermissions().ContainsKey(r.Name) ? SD.GetRolePermissions()[r.Name] : new List<string>()
                }).ToList();

                return new AssignRoleViewModel
                {
                    PositionId = positionId,
                    PositionName = position.Name,
                    AvailableRoles = availableRoles,
                    AssignedRoles = allRoles.Where(r => assignedRoleIds.Contains(r.Id)).Select(r => r.Name).ToList(),
                    SelectedRoleIds = assignedRoleIds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role assignment view");
                return null;
            }
        }

        public async Task<(bool Succeeded, string Message)> RemoveRoleFromPositionAsync(int positionId, string roleId)
        {
            try
            {
                if (positionId <= 0 || string.IsNullOrWhiteSpace(roleId))
                    return (false, SD.Messages.ERROR_INVALID_POSITION);

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

        #region Get Available Roles

        public async Task<List<RolePermissionDto>> GetAvailableRolesWithPermissionsAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var rolePermissions = SD.GetRolePermissions();

                return roles.Select(r => new RolePermissionDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Permissions = rolePermissions.ContainsKey(r.Name) ? rolePermissions[r.Name] : new List<string>()
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
                    .Include(e => e.EmployeePositions)
                    .ThenInclude(ep => ep.Position)
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
                    .Include(e => e.EmployeePositions)
                    .ThenInclude(ep => ep.Position)
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
                    ApplicationUserId = applicationUser.Id
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

                employee.FullName = $"{model.FirstName} {model.LastName}";
                employee.Phone = model.Phone;
                employee.Address = model.Address ?? string.Empty;

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
                    .Include(e => e.EmployeePositions)
                    .FirstOrDefaultAsync(x => x.Id == employeeId);

                if (employee == null)
                    return (false, "Employee not found");

                var applicationUserId = employee.ApplicationUserId;

                // Remove position assignments
                if (employee.EmployeePositions?.Count > 0)
                {
                    _context.EmployeePositions.RemoveRange(employee.EmployeePositions);
                }

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

        public async Task<(bool Succeeded, string Message)> AssignPositionToEmployeeAsync(int employeeId, int positionId)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.EmployeePositions)
                    .FirstOrDefaultAsync(x => x.Id == employeeId);

                if (employee == null)
                    return (false, "Employee not found");

                var position = await _context.PositionTables
                    .Include(p => p.PositionRoles)
                    .ThenInclude(pr => pr.Role)
                    .FirstOrDefaultAsync(x => x.Id == positionId);

                if (position == null)
                    return (false, "Position not found");

                // Check if already assigned
                var existingAssignment = await _context.EmployeePositions
                    .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.PositionId == positionId);

                if (existingAssignment != null)
                    return (false, "Employee already assigned to this position");

                var employeePosition = new EmployeePosition
                {
                    EmployeeId = employeeId,
                    PositionId = positionId
                };

                _context.EmployeePositions.Add(employeePosition);
                await _context.SaveChangesAsync();

                return (true, "Position assigned to employee successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning position {positionId} to employee {employeeId}");
                return (false, "Error assigning position");
            }
        }

        public async Task<(bool Succeeded, string Message)> RemovePositionFromEmployeeAsync(int employeeId, int positionId)
        {
            try
            {
                var employeePosition = await _context.EmployeePositions
                    .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.PositionId == positionId);

                if (employeePosition == null)
                    return (false, "Position assignment not found");

                _context.EmployeePositions.Remove(employeePosition);
                await _context.SaveChangesAsync();

                return (true, "Position removed from employee successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing position {positionId} from employee {employeeId}");
                return (false, "Error removing position");
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
                Positions = employee.EmployeePositions?
                    .Select(ep => ep.Position?.Name ?? string.Empty)
                    .Where(p => !string.IsNullOrEmpty(p))
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
                EmployeeCount = position.EmployeePositions?.Count ?? 0,
                AssignedRoles = position.PositionRoles?.Select(pr => pr.Role?.Name).Where(n => n != null).ToList() ?? new List<string>(),
                CreatedDate = position.CreatedDate,
                UpdatedDate = position.UpdatedDate
            };
        }

        #endregion
    }
}

