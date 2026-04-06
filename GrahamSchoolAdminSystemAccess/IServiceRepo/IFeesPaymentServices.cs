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
    public interface IFeesPaymentServices
    {
        /// <summary>
        /// Get all termly fees setup with pagination and filtering
        /// </summary>
        Task<(List<FeesPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetFeesPaymentsAsync(
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
        /// Get fees setup by ID
        /// </summary>
        Task<object> GetFeesPaymentByIdAsync(int id);

        /// <summary>
        /// Create new fees setup
        /// </summary>
        Task<ServiceResponse<int>> CreateFeesPaymentAsync(RecordPaymentViewModel record);

        /// <summary>
        /// Update fees setup
        /// </summary>
        Task<ServiceResponse<bool>> UpdateFeesPaymentAsync(FeesSetupViewModel model);

        /// <summary>
        /// Delete fees setup
        /// </summary>
        Task<ServiceResponse<bool>> DeleteFeesPaymentAsync(int id);

        Task<(RecordPaymentViewModel Data, bool Succecced, string Message)> SearchFeesPaymentAsync(RecordPaymeentSearchViewModel model);

        /// <summary>
        /// Get payment record by ID with all related data for invoice
        /// </summary>
        Task<FeesPaymentTable> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// Get total previous payments for a student term registration
        /// </summary>
        Task<decimal> GetTotalPreviousPaymentsAsync(int termRegId, int excludePaymentId = 0);

        /// <summary>
        /// Get previous balance for a student
        /// </summary>
        Task<decimal> GetPreviousBalanceAsync(int termRegId);

        /// <summary>
        /// Get fees payment report data
        /// </summary>
        Task<FeesReportViewModel> GetFeesReportAsync(int? sessionId = null, int? term = null, int? classId = null);

        /// <summary>
        /// Get payment detail with staff information for viewing
        /// </summary>
        Task<PaymentDetailViewModel> GetPaymentDetailAsync(int paymentId);

        /// <summary>
        /// Approve a fees payment
        /// </summary>
        Task<ServiceResponse<bool>> ApprovePaymentAsync(int paymentId);

        /// <summary>
        /// Reject a fees payment
        /// </summary>
        Task<ServiceResponse<bool>> RejectPaymentAsync(int paymentId);

        /// <summary>
        /// Cancel a fees payment (must be rejected first)
        /// </summary>
        Task<ServiceResponse<bool>> CancelPaymentAsync(int paymentId);
    }
}

