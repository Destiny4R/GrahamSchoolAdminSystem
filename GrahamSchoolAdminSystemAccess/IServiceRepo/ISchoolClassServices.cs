using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface ISchoolClassServices
    {
        /// <summary>
        /// Get all school classes with pagination and filtering
        /// </summary>
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetSchoolClassesAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc");

        /// <summary>
        /// Get school class by ID
        /// </summary>
        Task<SchoolClassViewModel> GetSchoolClassByIdAsync(int id);

        /// <summary>
        /// Get all school classes (without pagination)
        /// </summary>
        Task<List<SchoolClassViewModel>> GetAllSchoolClassesAsync();

        /// <summary>
        /// Create new school class
        /// </summary>
        Task<ServiceResponse<int>> CreateSchoolClassAsync(SchoolClassViewModel model);

        /// <summary>
        /// Update school class
        /// </summary>
        Task<ServiceResponse<bool>> UpdateSchoolClassAsync(SchoolClassViewModel model);

        /// <summary>
        /// Delete school class
        /// </summary>
        Task<ServiceResponse<bool>> DeleteSchoolClassAsync(int id);

        /// <summary>
        /// Check if class name already exists
        /// </summary>
        Task<bool> ClassNameExistsAsync(string name, int? excludeId = null);
    }
}
