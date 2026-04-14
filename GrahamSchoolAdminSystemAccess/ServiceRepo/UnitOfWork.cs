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
            IUsersServices usersServices,
            IStudentServices studentServices,
            ISystemActivitiesServices systemActivities,
            ILogService logService,
            ISchoolClassServices schoolClassServices,
            ITermRegistrationServices termRegistrationServices,
            IPaymentCategoryService paymentCategoryService,
            IPaymentItemService paymentItemService,
            IPaymentSetupService paymentSetupService,
            IStudentPaymentService studentPaymentService,
            IPaymentReportService paymentReportService)
        {
            UsersServices = usersServices;
            StudentServices = studentServices;
            SystemActivities = systemActivities;
            LogService = logService;
            SchoolClassServices = schoolClassServices;
            TermRegistrationServices = termRegistrationServices;
            PaymentCategoryService = paymentCategoryService;
            PaymentItemService = paymentItemService;
            PaymentSetupService = paymentSetupService;
            StudentPaymentService = studentPaymentService;
            PaymentReportService = paymentReportService;
        }

        public IUsersServices UsersServices { get; set; }
        public IStudentServices StudentServices { get; set; }
        public ISystemActivitiesServices SystemActivities { get; set; }
        public ILogService LogService { get; set; }
        public ISchoolClassServices SchoolClassServices { get; set; }
        public ITermRegistrationServices TermRegistrationServices { get; }
        public IPaymentCategoryService PaymentCategoryService { get; }
        public IPaymentItemService PaymentItemService { get; }
        public IPaymentSetupService PaymentSetupService { get; }
        public IStudentPaymentService StudentPaymentService { get; }
        public IPaymentReportService PaymentReportService { get; }
    }
}
