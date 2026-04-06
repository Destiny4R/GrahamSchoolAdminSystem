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
    public interface ITermRegistrationServices
    {
        Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> GetStudentTermRegistrationAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc",
            int? termFilter = null,
            int? sessionFilter = null,
            int? classFilter = null,
            int? subclassFilter = null);

        /// <summary>
        /// Get student term registration by ID
        /// </summary>
        Task<TermRegistrationViewModel> GetStudentTermRegistrationByIdAsync(int id);

        /// <summary>
        /// Get student by username for registration
        /// </summary>
        Task<StudentViewModel> GetStudentByUsernameAsync(string username);

        /// <summary>
        /// Get all student term registrations (without pagination)
        /// </summary>
        Task<List<TermRegistrationViewModel>> GetAllStudentTermRegistrationsAsync();

        /// <summary>
        /// Create new student term registration
        /// </summary>
        Task<ServiceResponse<int>> CreateStudentTermRegistrationAsync(TermRegistrationViewModel model);

        /// <summary>
        /// Update student term registration
        /// </summary>
        Task<ServiceResponse<bool>> UpdateStudentTermRegistrationAsync(TermRegistrationViewModel model);

        /// <summary>
        /// Delete student term registration
        /// </summary>
        Task<ServiceResponse<bool>> DeleteStudentTermRegistrationAsync(int id);

        /// <summary>
        /// Delete students term registration
        /// </summary>
        Task<ServiceResponse<bool>> DeleteStudentsTermRegistrationAsync(List<int> id);

        Task<List<TermRegistration>> GetOldTermRegRecordsAsync(SelectOptionsData Selectsearch);
    }
}
