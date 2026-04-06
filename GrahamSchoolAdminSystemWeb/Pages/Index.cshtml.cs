using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemWeb.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public DashboardViewModel DashboardData { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // ── General counts ─────────────────────────────────────────────
                var studentsCountTask = _context.Students.CountAsync();
                var employeesCountTask = _context.Employees.CountAsync();
                var classesCountTask = _context.SchoolClasses.CountAsync();

                // ── Termly Fees ────────────────────────────────────────────────
                var termlyFeesReportTask = _unitOfWork.FeesPaymentServices.GetFeesReportAsync();

                // ── PTA Fees ───────────────────────────────────────────────────
                var ptaFeesReportTask = _unitOfWork.PTAPaymentServices.GetPTAFeesReportAsync();

                // ── Other Payments (aggregate directly) ────────────────────────
                var otherApprovedTask = _context.OtherPayments
                    .Where(x => x.Status == PaymentStatus.Approved && x.PaymentState != PaymentState.Cancelled)
                    .SumAsync(x => (decimal?)x.Amount) ;

                var otherPendingAmtTask = _context.OtherPayments
                    .Where(x => x.Status == PaymentStatus.Pending && x.PaymentState != PaymentState.Cancelled)
                    .SumAsync(x => (decimal?)x.Amount);

                var otherApprovedCntTask = _context.OtherPayments
                    .CountAsync(x => x.Status == PaymentStatus.Approved && x.PaymentState != PaymentState.Cancelled);

                var otherPendingCntTask = _context.OtherPayments
                    .CountAsync(x => x.Status == PaymentStatus.Pending && x.PaymentState != PaymentState.Cancelled);

                // ── Termly Fees payment status counts ──────────────────────────
                var termlyApprovedCntTask = _context.FeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Approved && x.PaymentState != PaymentState.Cancelled);

                var termlyPendingCntTask = _context.FeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Pending && x.PaymentState != PaymentState.Cancelled);

                var termlyPartialCntTask = _context.FeesPayments
                    .CountAsync(x => x.PaymentState == PaymentState.PartPayment && x.Status != PaymentStatus.Rejected && x.PaymentState != PaymentState.Cancelled);

                // ── PTA payment status counts ──────────────────────────────────
                var ptaApprovedCntTask = _context.PTAFeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Approved && x.PaymentState != PaymentState.Cancelled);

                var ptaPendingCntTask = _context.PTAFeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Pending && x.PaymentState != PaymentState.Cancelled);

                // ── Rejected counts (all types combined) ──────────────────────
                var rejectedCntTask = _context.FeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Rejected);
                var rejectedPTACntTask = _context.PTAFeesPayments
                    .CountAsync(x => x.Status == PaymentStatus.Rejected);
                var rejectedOtherCntTask = _context.OtherPayments
                    .CountAsync(x => x.Status == PaymentStatus.Rejected);

                // ── Recent payments (last 8 across all types) ──────────────────
                var recentFeesTask = _context.FeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(tr => tr.Student)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(tr => tr.SchoolClass)
                    .Where(x => x.PaymentState != PaymentState.Cancelled)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new RecentPaymentItemViewModel
                    {
                        StudentName = x.TermRegistration.Student.Surname + " " + x.TermRegistration.Student.Firstname,
                        RegistrationNumber = x.TermRegistration.Student.ApplicationUser != null
                            ? x.TermRegistration.Student.ApplicationUser.UserName : "",
                        ClassName = x.TermRegistration.SchoolClass.Name,
                        PaymentType = "School Fees",
                        Amount = x.Amount,
                        Status = x.Status.ToString(),
                        PaymentState = x.PaymentState.ToString(),
                        InvoiceNumber = x.InvoiceNumber,
                        CreatedDate = x.CreatedDate
                    })
                    .ToListAsync();

                var recentPTATask = _context.PTAFeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(tr => tr.Student)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(tr => tr.SchoolClass)
                    .Where(x => x.PaymentState != PaymentState.Cancelled)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new RecentPaymentItemViewModel
                    {
                        StudentName = x.TermRegistration.Student.Surname + " " + x.TermRegistration.Student.Firstname,
                        RegistrationNumber = x.TermRegistration.Student.ApplicationUser != null
                            ? x.TermRegistration.Student.ApplicationUser.UserName : "",
                        ClassName = x.TermRegistration.SchoolClass.Name,
                        PaymentType = "PTA Levy",
                        Amount = x.Amount,
                        Status = x.Status.ToString(),
                        PaymentState = x.PaymentState.ToString(),
                        InvoiceNumber = x.InvoiceNumber,
                        CreatedDate = x.CreatedDate
                    })
                    .ToListAsync();

                var recentOtherTask = _context.OtherPayments
                    .Include(x => x.Termregistration)
                        .ThenInclude(tr => tr.Student)
                    .Include(x => x.Termregistration)
                        .ThenInclude(tr => tr.SchoolClass)
                    .Include(x => x.OtherPayFeesSetUp)
                        .ThenInclude(s => s.OtherPayItems)
                    .Where(x => x.PaymentState != PaymentState.Cancelled)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .Select(x => new RecentPaymentItemViewModel
                    {
                        StudentName = x.Termregistration.Student.Surname + " " + x.Termregistration.Student.Firstname,
                        RegistrationNumber = x.Termregistration.Student.ApplicationUser != null
                            ? x.Termregistration.Student.ApplicationUser.UserName : "",
                        ClassName = x.Termregistration.SchoolClass.Name,
                        PaymentType = x.OtherPayFeesSetUp != null && x.OtherPayFeesSetUp.OtherPayItems != null
                            ? x.OtherPayFeesSetUp.OtherPayItems.Name : "Other Payment",
                        Amount = x.Amount,
                        Status = x.Status.ToString(),
                        PaymentState = x.PaymentState.ToString(),
                        InvoiceNumber = x.InvoiceNumber,
                        CreatedDate = x.CreatedDate
                    })
                    .ToListAsync();

                // ── Current session label ──────────────────────────────────────
                var currentSessionTask = _context.SessionYears
                    .OrderByDescending(s => s.CreatedDate)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                // Await all tasks
                await Task.WhenAll(
                    studentsCountTask, employeesCountTask, classesCountTask,
                    termlyFeesReportTask, ptaFeesReportTask,
                    otherApprovedTask, otherPendingAmtTask, otherApprovedCntTask, otherPendingCntTask,
                    termlyApprovedCntTask, termlyPendingCntTask, termlyPartialCntTask,
                    ptaApprovedCntTask, ptaPendingCntTask,
                    rejectedCntTask, rejectedPTACntTask, rejectedOtherCntTask,
                    recentFeesTask, recentPTATask, recentOtherTask,
                    currentSessionTask
                );

                var termlyReport = await termlyFeesReportTask;
                var ptaReport = await ptaFeesReportTask;

                // Merge and sort recent payments (latest 8)
                var allRecent = (await recentFeesTask)
                    .Concat(await recentPTATask)
                    .Concat(await recentOtherTask)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(8)
                    .ToList();

                DashboardData = new DashboardViewModel
                {
                    TotalStudents = await studentsCountTask,
                    TotalEmployees = await employeesCountTask,
                    ActiveClasses = await classesCountTask,

                    // Termly fees
                    TermlyFeesExpected = termlyReport.TotalExpected,
                    TermlyFeesCollected = termlyReport.TotalActual,
                    TermlyFeesOutstanding = termlyReport.TotalOutstanding,
                    TermlyFeesApprovedCount = await termlyApprovedCntTask,
                    TermlyFeesPendingCount = await termlyPendingCntTask,
                    TermlyFeesPartialCount = await termlyPartialCntTask,

                    // PTA fees
                    PTAFeesExpected = ptaReport.TotalExpected,
                    PTAFeesCollected = ptaReport.TotalActual,
                    PTAFeesOutstanding = ptaReport.TotalOutstanding,
                    PTAFeesApprovedCount = await ptaApprovedCntTask,
                    PTAFeesPendingCount = await ptaPendingCntTask,

                    // Other payments
                    OtherPaymentsCollected = await otherApprovedTask ?? 0,
                    OtherPaymentsPending = await otherPendingAmtTask ?? 0,
                    OtherPaymentsApprovedCount = await otherApprovedCntTask,
                    OtherPaymentsPendingCount = await otherPendingCntTask,

                    // Rejected
                    TotalRejectedPayments = (await rejectedCntTask) + (await rejectedPTACntTask) + (await rejectedOtherCntTask),

                    RecentPayments = allRecent,
                    CurrentSessionLabel = await currentSessionTask ?? "Current Session"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                DashboardData = new DashboardViewModel();
            }
        }
    }
}
