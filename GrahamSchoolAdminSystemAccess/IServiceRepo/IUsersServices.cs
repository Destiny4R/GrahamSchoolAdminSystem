using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.ViewModels;
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
        Task<(List<PositionDto> data, int recordsTotal, int recordsFiltered)> GetPositionsAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection);

        // Position-Role Assignment
        Task<(bool Succeeded, string Message)> AssignRolesToPositionAsync(int positionId, List<string> roleIds);
        Task<AssignRoleViewModel> GetRoleAssignmentViewAsync(int positionId);
        Task<(bool Succeeded, string Message)> RemoveRoleFromPositionAsync(int positionId, string roleId);

        // Get available roles with permissions
        Task<List<RolePermissionDto>> GetAvailableRolesWithPermissionsAsync();

        // Employee Management
        Task<(List<EmployeeViewModel> data, int recordsTotal, int recordsFiltered)> GetEmployeesAsync(int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc");
        Task<EmployeeViewModel> GetEmployeeByIdAsync(int employeeId);
        Task<(bool Succeeded, string Message, object Data)> CreateEmployeeAsync(EmployeeViewModel model);
        Task<(bool Succeeded, string Message)> UpdateEmployeeAsync(EmployeeViewModel model);
        Task<(bool Succeeded, string Message)> DeleteEmployeeAsync(int employeeId);
        Task<(bool Succeeded, string Message)> AssignPositionToEmployeeAsync(int employeeId, int positionId);
        Task<(bool Succeeded, string Message)> RemovePositionFromEmployeeAsync(int employeeId, int positionId);
    }
}
