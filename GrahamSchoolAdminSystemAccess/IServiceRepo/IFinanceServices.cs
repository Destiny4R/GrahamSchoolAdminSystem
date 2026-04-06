using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IFinanceServices
    {
        /// <summary>
        /// Get all termly fees setup with pagination and filtering
        /// </summary>
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetFeesSetupAsync(
            int skip = 0, 
            int pageSize = 10, 
            string searchTerm = "", 
            int sortColumn = 0, 
            string sortDirection = "asc");

        /// <summary>
        /// Get fees setup by ID
        /// </summary>
        Task<FeesSetupViewModel> GetFeesSetupByIdAsync(int id);

        /// <summary>
        /// Create new fees setup
        /// </summary>
        Task<ServiceResponse<int>> CreateFeesSetupAsync(FeesSetupViewModel model);

        /// <summary>
        /// Update fees setup
        /// </summary>
        Task<ServiceResponse<bool>> UpdateFeesSetupAsync(FeesSetupViewModel model);

        /// <summary>
        /// Delete fees setup
        /// </summary>
        Task<ServiceResponse<bool>> DeleteFeesSetupAsync(int id);

        /// <summary>
        /// Get dropdown data for fees setup form
        /// </summary>
        Task<ViewSelections> GetFeesSetupSelectionsAsync();

        #region PTA Fees Setup Methods

        /// <summary>
        /// Get all PTA fees setup with pagination and filtering
        /// </summary>
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPTAFeesSetupAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc");

        /// <summary>
        /// Get PTA fees setup by ID
        /// </summary>
        Task<PTAFeesSetupViewModel> GetPTAFeesSetupByIdAsync(int id);

        /// <summary>
        /// Create new PTA fees setup
        /// </summary>
        Task<ServiceResponse<int>> CreatePTAFeesSetupAsync(PTAFeesSetupViewModel model);

        /// <summary>
        /// Update PTA fees setup
        /// </summary>
        Task<ServiceResponse<bool>> UpdatePTAFeesSetupAsync(PTAFeesSetupViewModel model);

        /// <summary>
        /// Delete PTA fees setup
        /// </summary>
        Task<ServiceResponse<bool>> DeletePTAFeesSetupAsync(int id);

        /// <summary>
        /// Get dropdown data for PTA fees setup form
        /// </summary>
        Task<ViewSelections> GetPTAFeesSetupSelectionsAsync();

        /// <summary>
        /// Get dropdown data for PTA payment form
        /// </summary>
        Task<ViewSelections> GetPTAPaymentSelectionsAsync();

        #endregion

        #region Other Fees Setup Methods

        /// <summary>
        /// Get all other fees setup with pagination and filtering
        /// </summary>
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetOtherFeesSetupAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc");

        /// <summary>
        /// Get other fees setup by ID
        /// </summary>
        Task<OtherFeesSetupViewModel> GetOtherFeesSetupByIdAsync(int id);

        /// <summary>
        /// Create new other fees setup
        /// </summary>
        Task<ServiceResponse<int>> CreateOtherFeesSetupAsync(OtherFeesSetupViewModel model);

        /// <summary>
        /// Update other fees setup
        /// </summary>
        Task<ServiceResponse<bool>> UpdateOtherFeesSetupAsync(OtherFeesSetupViewModel model);

        /// <summary>
        /// Delete other fees setup
        /// </summary>
        Task<ServiceResponse<bool>> DeleteOtherFeesSetupAsync(int id);

        /// <summary>
        /// Get dropdown data for other fees setup form
        /// </summary>
        Task<ViewSelections> GetOtherFeesSetupSelectionsAsync();

        /// <summary>
        /// Get dropdown data for other payment form
        /// </summary>
        Task<ViewSelections> GetOtherPaymentSelectionsAsync();

        #endregion
    }
}
