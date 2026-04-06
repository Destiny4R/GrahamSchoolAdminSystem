using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface ISystemActivitiesServices
    {
        //School class interfaces
        Task<(List<SchoolClassesDto> data, int recordsTotal, int recordsFiltered)> GetSchoolClassesAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection);
        Task<(bool Succeeded, string Message)> CreateSchoolClassAsync(SchoolClassViewModel model);
        Task<(bool Succeeded, string Message)> UpdateSchoolClassAsync(SchoolClassViewModel model);
        Task<(bool Succeeded, string Message)> DeleteSchoolClassAsync(int id, string message);

        //School sub class interface
        Task<(List<SchoolSubClassDto> data, int recordsTotal, int recordsFiltered)> GetSchoolSubClassesAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection);
        Task<(bool Succeeded, string Message)> CreateSchoolSubClassAsync(SchoolSubClassViewModel model);
        Task<(bool Succeeded, string Message)> UpdateSchoolSubClassAsync(SchoolSubClassViewModel model);
        Task<(bool Succeeded, string Message)> DeleteSchoolSunClassAsync(int id, string message);

        //Academic session interfaces
        Task<(List<SessionYearDto> data, int recordsTotal, int recordsFiltered)> GetAcademicSessionAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection);
        Task<(bool Succeeded, string Message)> CreateAcademicSessionAsync(SessionYearViewModel model);
        Task<(bool Succeeded, string Message)> UpdateAcademicSessionAsync(SessionYearViewModel model);
        Task<(bool Succeeded, string Message)> DeleteAcademicSessionAsync(int id, string message);
    }
}
