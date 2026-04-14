using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IStudentPaymentService
    {
        Task<MakePaymentPageViewModel> GetPayableItemsAsync(int termRegistrationId);
        Task<ServiceResponse<MakePaymentPageViewModel>> LookupPayableItemsAsync(string admissionNo, int classId, int categoryId);
        Task<ServiceResponse<int>> CreatePaymentAsync(CreatePaymentViewModel model, string? evidenceFilePath = null);
        Task<PaymentReceiptViewModel> GetReceiptAsync(int paymentId);
        Task<PaymentReceiptViewModel> GetPaymentDetailAsync(int paymentId);
        Task<ServiceResponse<bool>> UpdatePaymentStateAsync(int paymentId, PaymentState state, string? rejectMessage);
        Task<ConsolidatedReceiptViewModel> GetConsolidatedReceiptAsync(int termRegId);
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentsDataTableAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null,
            string? statusFilter = null, int? stateFilter = null);
        Task<List<PendingPaymentNotification>> GetPendingPaymentNotificationsAsync(int maxCount = 20);
    }
}
