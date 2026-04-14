using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IPaymentCategoryService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentCategoriesAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc");
        Task<PaymentCategoryViewModel> GetPaymentCategoryByIdAsync(int id);
        Task<List<PaymentCategoryViewModel>> GetActiveCategoriesAsync();
        Task<ServiceResponse<int>> CreatePaymentCategoryAsync(PaymentCategoryViewModel model);
        Task<ServiceResponse<bool>> UpdatePaymentCategoryAsync(PaymentCategoryViewModel model);
        Task<ServiceResponse<bool>> DeletePaymentCategoryAsync(int id);
        Task<ServiceResponse<bool>> TogglePaymentCategoryAsync(int id);
    }
}
