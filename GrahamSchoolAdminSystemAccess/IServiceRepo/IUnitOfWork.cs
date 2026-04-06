using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IUnitOfWork
    {
        IFinanceServices FinanceServices { get; }
        IUsersServices UsersServices { get; }
        IStudentServices StudentServices { get; }
        ISystemActivitiesServices SystemActivities { get; }
        ILogService LogService { get; }
        ISchoolClassServices SchoolClassServices { get; }
        ITermRegistrationServices TermRegistrationServices { get; }
        IFeesPaymentServices FeesPaymentServices { get; }
        IPTAPaymentServices PTAPaymentServices { get; }
        IOtherPaymentServices OtherPaymentServices { get; }
    }
}
