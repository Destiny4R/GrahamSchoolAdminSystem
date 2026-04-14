using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IUnitOfWork
    {
        IUsersServices UsersServices { get; }
        IStudentServices StudentServices { get; }
        ISystemActivitiesServices SystemActivities { get; }
        ILogService LogService { get; }
        ISchoolClassServices SchoolClassServices { get; }
        ITermRegistrationServices TermRegistrationServices { get; }
        IPaymentCategoryService PaymentCategoryService { get; }
        IPaymentItemService PaymentItemService { get; }
        IPaymentSetupService PaymentSetupService { get; }
        IStudentPaymentService StudentPaymentService { get; }
        IPaymentReportService PaymentReportService { get; }
    }
}
