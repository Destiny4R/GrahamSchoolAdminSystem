using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IOtherPaymentServices
    {
        #region OTHER PAYMENT ITEMS INTERFACE
        Task<(List<OtherPayItemsDtos> data, int recordsTotal, int recordsFiltered)> GetOtherItemsListAsync(
            int start = 0,
            int length = 10,
            string searchValue = "",
            int sortColumnIndex = 0,
            string sortDirection = "asc");

        Task<OtherPayItemsTable> GetOtherItemByIdAsync(int id);

        Task<(bool Succeeded, string Message, object Data)> CreateAndUpdateItemAsync(OtherPayItemsViewModel model);

        Task<(bool Succeeded, string Message)> DeleteOtherItemAsync(int id);

        IEnumerable<SelectListItem> OtherPaymentItemsOptions();
        #endregion

        // Other Payment lifecycle methods
        Task<ServiceResponse<int>> CreateOtherPaymentAsync(RecordPaymentViewModel record);

        Task<(List<FeesPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetOtherPaymentsAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc",
            int? termFilter = null,
            int? sessionFilter = null,
            int? classFilter = null,
            int? subclassFilter = null);

        Task<OtherPayment> GetOtherPaymentByIdAsync(int paymentId);

        Task<decimal> GetTotalPreviousOtherPaymentsAsync(int termRegId, int excludePaymentId = 0);

        Task<decimal> GetPreviousBalanceAsync(int termRegId);

        Task<List<OtherPayment>> GetApprovedPaymentsByItemAsync(int termRegistrationId, int paymentSetUpId);

        Task<ServiceResponse<bool>> ApproveOtherPaymentAsync(int paymentId);

        Task<ServiceResponse<bool>> RejectOtherPaymentAsync(int paymentId);

        Task<ServiceResponse<bool>> CancelOtherPaymentAsync(int paymentId);

        Task<PaymentDetailViewModel> GetOtherPaymentDetailAsync(int paymentId);

        Task<ServiceResponse<RecordPaymentViewModel>> SearchOtherPaymentAsync(RecordPaymeentSearchViewModel searchModel);
        #region OTHER FEES SET UP
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetOtherFeesSetUpAsync(
            int start = 0,
            int length = 10,
            string searchValue = "",
            int sortColumnIndex = 0,
            string sortDirection = "asc");
        Task<OtherPayFeesSetUp> GetOtherFeesSetUpByIdAsync(int paymentId);
        Task<(bool Succeeded, string Message)> DeleteOtherFeeSetUpAsync(int id);
        #endregion
    }
}
