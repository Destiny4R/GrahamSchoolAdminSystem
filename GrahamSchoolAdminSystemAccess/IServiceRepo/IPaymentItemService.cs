using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IPaymentItemService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentItemsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? categoryFilter = null);
        Task<PaymentItemViewModel> GetPaymentItemByIdAsync(int id);
        Task<List<PaymentItemViewModel>> GetActiveItemsAsync(int? categoryId = null);
        Task<ServiceResponse<int>> CreatePaymentItemAsync(PaymentItemViewModel model);
        Task<ServiceResponse<bool>> UpdatePaymentItemAsync(PaymentItemViewModel model);
        Task<ServiceResponse<bool>> DeletePaymentItemAsync(int id);
        Task<ServiceResponse<bool>> TogglePaymentItemAsync(int id);
    }
}
