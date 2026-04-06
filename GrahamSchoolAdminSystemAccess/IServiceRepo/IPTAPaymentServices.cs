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
    public interface IPTAPaymentServices
    {
        /// <summary>
        /// Create a new PTA payment record
        /// </summary>
        Task<ServiceResponse<int>> CreatePTAPaymentAsync(RecordPTAPaymentViewModel record);

        /// <summary>
        /// Get paginated list of PTA payments with filtering and sorting
        /// </summary>
        Task<(List<PTAPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetPTAPaymentsAsync(
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
        /// Search for PTA payment information
        /// </summary>
        Task<(RecordPTAPaymentViewModel Data, bool Succeeded, string Message)> SearchPTAPaymentAsync(RecordPTAPaymentSearchViewModel model);

        /// <summary>
        /// Get PTA payment detail with staff information
        /// </summary>
        Task<PTAPaymentDetailViewModel> GetPTAPaymentDetailAsync(int paymentId);

        /// <summary>
        /// Get total previous PTA payments for a student
        /// </summary>
        Task<decimal> GetTotalPreviousPTAPaymentsAsync(int termRegId, int excludePaymentId = 0);

        /// <summary>
        /// Get previous balance (outstanding amount) for a student term registration
        /// </summary>
        Task<decimal> GetPreviousBalanceAsync(int termRegId);

        /// <summary>
        /// Get PTA fees report data
        /// </summary>
        Task<PTAFeesReportViewModel> GetPTAFeesReportAsync(int? sessionId = null, int? term = null, int? classId = null);

        /// <summary>
        /// Approve a PTA payment
        /// </summary>
        Task<ServiceResponse<bool>> ApprovePTAPaymentAsync(int paymentId);

        /// <summary>
        /// Reject a PTA payment
        /// </summary>
        Task<ServiceResponse<bool>> RejectPTAPaymentAsync(int paymentId);

        /// <summary>
        /// Cancel a PTA payment (must be rejected first)
        /// </summary>
        Task<ServiceResponse<bool>> CancelPTAPaymentAsync(int paymentId);

        /// <summary>
        /// Get PTA payment by ID for invoice generation
        /// </summary>
        Task<PTAFeesPayments> GetPTAPaymentByIdAsync(int paymentId);
    }
}
