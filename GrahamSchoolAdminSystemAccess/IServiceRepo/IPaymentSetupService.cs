using GrahamSchoolAdminSystemModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IPaymentSetupService
    {
        Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentSetupsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null);
        Task<PaymentSetupViewModel> GetPaymentSetupByIdAsync(int id);
        Task<ServiceResponse<int>> CreatePaymentSetupAsync(PaymentSetupViewModel model);
        Task<ServiceResponse<bool>> UpdatePaymentSetupAsync(PaymentSetupViewModel model);
        Task<ServiceResponse<bool>> DeletePaymentSetupAsync(int id);
        Task<ServiceResponse<bool>> TogglePaymentSetupAsync(int id);
    }
}
