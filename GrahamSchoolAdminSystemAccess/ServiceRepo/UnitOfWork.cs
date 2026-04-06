using GrahamSchoolAdminSystemAccess.IServiceRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(
            IFinanceServices financeServices,
            IUsersServices usersServices,
            IStudentServices studentServices,
            ISystemActivitiesServices systemActivities,
            ILogService logService,
            ISchoolClassServices schoolClassServices,
            ITermRegistrationServices termRegistrationServices,
            IFeesPaymentServices feesPaymentServices,
            IPTAPaymentServices ptaPaymentServices,
            IOtherPaymentServices otherPaymentServices)
        {
            FinanceServices = financeServices;
            UsersServices = usersServices;
            StudentServices = studentServices;
            SystemActivities = systemActivities;
            LogService = logService;
            SchoolClassServices = schoolClassServices;
            TermRegistrationServices = termRegistrationServices;
            FeesPaymentServices = feesPaymentServices;
            PTAPaymentServices = ptaPaymentServices;
            OtherPaymentServices = otherPaymentServices;
        }

        public IFinanceServices FinanceServices { get; set; }
        public IUsersServices UsersServices { get; set; }
        public IStudentServices StudentServices { get; set; }
        public ISystemActivitiesServices SystemActivities { get; set; }
        public ILogService LogService { get; set; }
        public ISchoolClassServices SchoolClassServices { get; set; }
        public ITermRegistrationServices TermRegistrationServices { get; }
        public IFeesPaymentServices FeesPaymentServices { get; }
        public IPTAPaymentServices PTAPaymentServices { get; }
        public IOtherPaymentServices OtherPaymentServices { get; }
    }
}
