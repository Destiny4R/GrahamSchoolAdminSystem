using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IUsersServices
    {
        // Position Management
        Task<(bool Succeeded, string Message)> CreatePositionAsync(PositionViewModel model);
        Task<(bool Succeeded, string Message)> UpdatePositionAsync(PositionViewModel model);
        Task<(bool Succeeded, string Message)> DeletePositionAsync(int positionId);
        Task<PositionDto> GetPositionByIdAsync(int positionId);
        IEnumerable<SelectListItem> PositionsList();
        Task<(List<PositionDto> data, int recordsTotal, int recordsFiltered)> GetPositionsAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection);

        // Position-Role Assignment
        Task<(bool Succeeded, string Message)> AssignRolesToPositionAsync(int positionId, List<string> roleIds);
        Task<AssignRoleViewModel> GetRoleAssignmentViewAsync(int positionId);
        Task<(bool Succeeded, string Message)> RemoveRoleFromPositionAsync(int positionId, string roleId);

        // Role-Permission Management
        Task<(bool Succeeded, string Message)> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds);

        // Get available roles with permissions (DB-driven)
        Task<List<RolePermissionDto>> GetAvailableRolesWithPermissionsAsync();

        // Employee Management
        Task<(List<EmployeeViewModel> data, int recordsTotal, int recordsFiltered)> GetEmployeesAsync(int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc");
        Task<EmployeeViewModel> GetEmployeeByIdAsync(int employeeId);
        Task<(bool Succeeded, string Message, object Data)> CreateEmployeeAsync(EmployeeViewModel model);
        Task<(bool Succeeded, string Message)> UpdateEmployeeAsync(EmployeeViewModel model);
        Task<(bool Succeeded, string Message)> DeleteEmployeeAsync(int employeeId);

        // Password Management
        Task<(bool Succeeded, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        #region App Settings
        Task<(bool Succeeded, string Message)> CreateOrUpdateAppSettingsAsync(AppSettingViewModel model);
        Task<AppSettingViewModel> GetAppSettingsByUserIdAsync();
        #endregion
    }
}
