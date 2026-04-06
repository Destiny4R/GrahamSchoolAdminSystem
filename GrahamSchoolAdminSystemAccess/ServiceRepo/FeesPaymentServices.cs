using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class FeesPaymentServices : IFeesPaymentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeesPaymentServices> _logger;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeesPaymentServices(ApplicationDbContext context, ILogger<FeesPaymentServices> logger, ILogService logService, IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            this._logger = logger;
            this._logService = logService;
            this._httpContextAccessor = httpContextAccessor;
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from HttpContext
        /// </summary>
        private string GetCurrentUserId()
        {
            return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        /// <summary>
        /// Get current user name from HttpContext
        /// </summary>
        private string GetCurrentUserName()
        {
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";
        }

        /// <summary>
        /// Get client IP address from HttpContext
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext == null)
                    return "Unknown";

                if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                    return httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

                return httpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion
        public async Task<ServiceResponse<int>> CreateFeesPaymentAsync(RecordPaymentViewModel record)
        {
            try
            {
                // 1. Basic validation
                if (record.amount < 100)
                    return ServiceResponse<int>.Failure("The fee payment amount is too low to be processed.");

                // 2. Fetch required data
                var feesSetup = await _context.TermlyFeesSetups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == record.FeesSetUpid);

                if (feesSetup == null)
                    return ServiceResponse<int>.Failure("Term fees have not yet been configured for the selected class.");

                var studentReg = await _context.TermRegistrations
                    .Include(x => x.Student.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == record.termregid);

                if (studentReg == null)
                    return ServiceResponse<int>.Failure("No student was found with the provided registration number.");

                // 3. Get total previous payments (optimized)
                var totalPaidBefore = await _context.FeesPayments
                    .Where(x => x.TermRegId == record.termregid && x.PaymentState != PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var newTotal = totalPaidBefore + record.amount;

                // 4. Prevent overpayment

                // Rule 1: Single payment cannot exceed total fees
                if (record.amount > feesSetup.Amount)
                {
                    return ServiceResponse<int>.Failure("Payment amount cannot exceed total school fees.");
                }

                // Rule 2: Cumulative payment cannot exceed total fees
                if (newTotal > feesSetup.Amount)
                {
                    var remainingBalance = feesSetup.Amount - totalPaidBefore;

                    return ServiceResponse<int>.Failure(
                        $"Payment exceeds remaining balance. Remaining balance is {remainingBalance:N2}."
                    );
                }

                // 5. Handle file upload
                var uploadDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "feesFiles"
                );

                var fileUploadResult = FileUploadHandler.ProcessFile(record.evidenncefile, uploadDirectory);

                if (!fileUploadResult.Success)
                    return ServiceResponse<int>.Failure(fileUploadResult.Message);

                // 6. Determine payment state
                var paymentState = newTotal == feesSetup.Amount
                    ? PaymentState.Completed
                    : PaymentState.PartPayment;

                // 7. Create payment entity
                var payment = new FeesPaymentTable
                {
                    Amount = record.amount,
                    TermlyFeesId = record.FeesSetUpid,
                    TermRegId = record.termregid,
                    Fees = feesSetup.Amount,
                    Narration = record.narration ?? string.Empty,
                    FilePath = fileUploadResult.OutputFilePath,
                    PaymentState = paymentState,
                    InvoiceNumber = $"PAY-{SD.GenerateUniqueNumber()}",
                    StaffUserId = GetCurrentUserId()
                };

                _context.FeesPayments.Add(payment);

                // 8. Save changes
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    return ServiceResponse<int>.Failure("An error occurred while recording the fees payment. Please try again.");

                // 9. Log activity
                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Fees Payment",
                    entityType: "FeesPayment",
                    entityId: payment.Id.ToString(),
                    message: "Fees payment recorded successfully and is pending approval.",
                    ipAddress: GetClientIpAddress(),
                    details: $"Student Reg Number: {record.regnumber}, Session ID: {record.session}, Term: {record.term}"
                );

                return ServiceResponse<int>.Success(payment.Id, "Fees payment recorded successfully and is pending approval.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fees payment");
                return ServiceResponse<int>.Failure("An unexpected error occurred while processing the fees payment.");
            }
        }
        public async Task<ServiceResponse<int>> CreateFeesPaymentAsync2(RecordPaymentViewModel record)
        {
            //Wrap in try catch block to handle any unexpected errors
            try
            {
                if (record.amount < 100)
                {
                    return ServiceResponse<int>.Failure("The fee payment amount is too low to be processed.");
                }
                var feessetup = await _context.TermlyFeesSetups.FirstOrDefaultAsync(k => k.Id == record.FeesSetUpid);
                if (feessetup == null)
                {
                    return ServiceResponse<int>.Failure("Term fees have not yet been configured for the selected class.");
                }
                var studentinfo = await _context.TermRegistrations.Include(k => k.Student.ApplicationUser).FirstOrDefaultAsync(k => k.Id == record.termregid);
                if (studentinfo == null)
                {
                    return ServiceResponse<int>.Failure("No student was found with the provided registration number.");
                }

                //var fileUpload = FileUploadHandler.ProcessFile(record.evidenncefile, "feesFiles/");
                string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "feesFiles");

                // Process and save the file
                var fileUploadResult = FileUploadHandler.ProcessFile(record.evidenncefile, uploadDirectory);
                if (!fileUploadResult.Success)
                {
                    return ServiceResponse<int>.Failure(fileUploadResult.Message);
                }

                FeesPaymentTable pay = new()
                {
                    Amount = record.amount,
                    TermlyFeesId = record.FeesSetUpid,
                    TermRegId = record.termregid,
                    Fees = feessetup.Amount,
                    Message = record.narration ?? string.Empty,
                    FilePath = $"feesFiles/{fileUploadResult.FileName}",//fileUploadResult.OutputFilePath,
                    InvoiceNumber = $"PAY-{SD.GenerateUniqueNumber()}",
                };
                var ifPaybefore = _context.FeesPayments.Include(p => p.TermRegistration).Where(k => k.TermRegId == record.termregid && k.PaymentState != GetEnums.PaymentState.Cancelled && k.Status != PaymentStatus.Rejected).ToList();
                if (ifPaybefore.Any()) 
                {
                    var totalamount = ifPaybefore.Sum(k => k.Amount);
                    var finalamount = totalamount + record.amount;

                    if (finalamount == feessetup.Amount)
                    {
                        pay.PaymentState = PaymentState.Completed;
                    }else
                        pay.PaymentState = PaymentState.PartPayment;
                }
                if(record.amount == feessetup.Amount)
                    pay.PaymentState = PaymentState.Completed;
                else
                    pay.PaymentState = PaymentState.PartPayment;
                //Save to database
                _context.FeesPayments.Add(pay);
                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    //Log the transaction in ILogServices
                    //Adjust the log details as necessary to capture relevant information about the fees payment
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Fees Payment",
                        entityType: "FeesPayment",
                        entityId: pay.Id.ToString(),
                        message: "Fees payment recorded successfully and is pending approval.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Student Reg Number: {record.regnumber}, Session ID: {record.session}, Term: {record.term}"
                    );

                    return ServiceResponse<int>.Success(pay.Id, "Fees payment recorded successfully and is pending approval.");
                }
                else
                {
                    return ServiceResponse<int>.Failure("An error occurred while recording the fees payment. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating fees payment: {ex.Message}");
                return ServiceResponse<int>.Failure("An unexpected error occurred while processing the fees payment. Please try again later.");
            }
        
        }
        //ViewSelections Selections
        public async Task<(RecordPaymentViewModel Data, bool Succecced, string Message)> SearchFeesPaymentAsync(RecordPaymeentSearchViewModel model)
        {
            var feessetup = await _context.TermlyFeesSetups.FirstOrDefaultAsync(k => (int)k.Term == model.term && k.SessionId == model.sessionid && k.SchoolClassId == model.schoolclass);
            if (feessetup == null)
            {
                return (null, false, "Term fees have not yet been configured for the selected class.");
            }
            var studentinfo = await _context.TermRegistrations.Include(k => k.Student.ApplicationUser).Include(k=>k.SessionYear).Include(k => k.SchoolClass).Include(k => k.SchoolSubClass).FirstOrDefaultAsync(k => k.Student.ApplicationUser.UserName == model.StudentRegNumber && (int)k.Term == model.term && k.SessionId == model.sessionid && k.SchoolClassId == model.schoolclass);
            if (studentinfo == null)
            {
                return (null, false, "No student was found with the provided registration number.");
            }

            RecordPaymentViewModel recordPaymentViewModel = new()
            {
                term = feessetup.Term.ToString(),
                termregid = studentinfo.Id,
                FeesSetUpid = feessetup.Id,
                name = studentinfo.Student.FullName,
                session = studentinfo.SessionYear.Name,
                schoolclass = $"{studentinfo.SchoolClass.Name} - {studentinfo.SchoolSubClass.Name }",
                regnumber = studentinfo.Student.ApplicationUser.UserName,
                feespayment = feessetup.Amount,
            };


            //Check if there a part payment made for this registration number, if Yes calculate balance payment
            var ifPaybefore = _context.FeesPayments.Include(p => p.TermRegistration).Where(k => k.TermRegistration.Student.ApplicationUser.UserName == model.StudentRegNumber && (int)k.TermRegistration.Term == model.term && k.TermRegistration.SessionId == model.sessionid && k.TermRegistration.SchoolClassId == model.schoolclass && k.PaymentState!= GetEnums.PaymentState.Cancelled && k.Status != PaymentStatus.Rejected).ToList();
            if (ifPaybefore.Any())
            {
                recordPaymentViewModel.balance = ifPaybefore.Sum(k => k.Amount);
                
            }
            return (recordPaymentViewModel, true, "Retrieved successfully.");
        }

        public Task<ServiceResponse<bool>> DeleteFeesPaymentAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> GetFeesPaymentByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<(List<FeesPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetFeesPaymentsAsync(int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc", int? termFilter = null, int? sessionFilter = null, int? classFilter = null, int? subclassFilter = null)
        {
            try
            {
                var query = _context.FeesPayments
                    .Include(tr => tr.TermRegistration.Student.ApplicationUser)
                    .Include(tr => tr.TermRegistration.SchoolClass)
                    .Include(tr => tr.TermRegistration.SessionYear)
                    .Include(tr => tr.TermRegistration.SchoolSubClass)
                    .Include(tr => tr.TermlyFeesSetup)
                    .AsNoTracking()
                    .OrderByDescending(k => k.CreatedDate)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.TermRegistration.Student.Surname.Contains(searchTerm) ||
                        x.TermRegistration.Student.Firstname.Contains(searchTerm) ||
                        x.TermRegistration.SchoolClass.Name.Contains(searchTerm) ||
                        x.TermRegistration.SessionYear.Name.Contains(searchTerm));
                }

                // Apply filters if provided
                if (termFilter.HasValue && termFilter > 0)
                {
                    query = query.Where(x => (int)x.TermRegistration.Term == termFilter.Value);
                }

                if (sessionFilter.HasValue && sessionFilter > 0)
                {
                    query = query.Where(x => x.TermRegistration.SessionId == sessionFilter.Value);
                }

                if (classFilter.HasValue && classFilter > 0)
                {
                    query = query.Where(x => x.TermRegistration.SchoolClassId == classFilter.Value);
                }

                if (subclassFilter.HasValue && subclassFilter > 0)
                {
                    query = query.Where(x => x.TermRegistration.SchoolSubclassId == subclassFilter.Value);
                }

                var recordsFiltered = await query.CountAsync();

                var sortColumnName = sortColumn switch
                {
                    0 => nameof(TermRegistration.Id),
                    1 => nameof(TermRegistration.StudentId),
                    2 => nameof(TermRegistration.SchoolClassId),
                    3 => nameof(TermRegistration.SessionId),
                    4 => nameof(TermRegistration.Term),
                    _ => nameof(TermRegistration.CreatedDate)
                };

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderBy(x => EF.Property<object>(x, sortColumnName))
                    : query.OrderByDescending(x => EF.Property<object>(x, sortColumnName));

                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new FeesPaymentsDto
                    {

                        id = x.TermRegistration.Id,
                        name = $"{x.TermRegistration.Student.FullName}",
                        term = x.TermRegistration.Term.ToString(),
                        session = x.TermRegistration.SessionYear.Name,
                        schoolclass = x.TermRegistration.SchoolClass.Name,
                        createdate = x.TermRegistration.CreatedDate,
                        regnumber = x.TermRegistration.Student.ApplicationUser.UserName,
                        fees = SD.ToNaira(x.TermlyFeesSetup.Amount),
                        amount = SD.ToNaira(x.Amount),
                        balance = SD.ToNaira((x.TermlyFeesSetup.Amount - x.Amount)),
                        state = SD.GetSpanBadgeState(x.PaymentState),
                        //Get value of Enum and convert value to enum name
                        status = SD.GetSpanBadgeStatus(x.Status)
                    })
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting term registrations: {ex.Message}");
                return (new List<FeesPaymentsDto>(), 0, 0);
            }
        }

        public Task<ServiceResponse<bool>> UpdateFeesPaymentAsync(FeesSetupViewModel model)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get payment record by ID with all related data for invoice
        /// </summary>
        public async Task<FeesPaymentTable> GetPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _context.FeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.TermlyFeesSetup)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get total previous payments for a student term registration excluding current payment
        /// </summary>
        public async Task<decimal> GetTotalPreviousPaymentsAsync(int termRegId, int excludePaymentId = 0)
        {
            try
            {
                var total = await _context.FeesPayments
                    .Where(x => x.TermRegId == termRegId && x.Id != excludePaymentId && x.PaymentState != PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total previous payments: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get previous balance for a student (total fees - total paid for all previous payments)
        /// </summary>
        public async Task<decimal> GetPreviousBalanceAsync(int termRegId)
        {
            try
            {
                var feesPayment = await _context.FeesPayments
                    .Where(x => x.TermRegId == termRegId && x.PaymentState != PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .FirstOrDefaultAsync();

                if (feesPayment == null)
                    return 0;

                var totalPaid = await _context.FeesPayments
                    .Where(x => x.TermRegId == termRegId && x.PaymentState != PaymentState.Cancelled && x.Status != PaymentStatus.Rejected && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var balance = feesPayment.Fees - totalPaid;
                return balance > 0 ? balance : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting previous balance: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get fees report data for sessions, terms, and classes
        /// </summary>
        public async Task<FeesReportViewModel> GetFeesReportAsync(int? sessionId = null, int? term = null, int? classId = null)
        {
            try
            {
                var reportViewModel = new FeesReportViewModel();

                // Get all sessions
                reportViewModel.Sessions = await _context.SessionYears
                    .Select(s => new SessionDropdownViewModel
                    {
                        Id = s.Id,
                        Name = s.Name
                    })
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                // Get all terms (hardcoded from enum)
                reportViewModel.Terms = new List<TermDropdownViewModel>
                {
                    new TermDropdownViewModel { Value = 1, Name = "First" },
                    new TermDropdownViewModel { Value = 2, Name = "Second" },
                    new TermDropdownViewModel { Value = 3, Name = "Third" }
                };

                // Get all classes
                reportViewModel.Classes = await _context.SchoolClasses
                    .Select(c => new ClassDropdownViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                // Build report data based on filters
                var sessionsToQuery = sessionId.HasValue
                    ? _context.SessionYears.Where(s => s.Id == sessionId.Value)
                    : _context.SessionYears;

                var reportData = new List<FeesReportLineItemViewModel>();

                foreach (var session in await sessionsToQuery.ToListAsync())
                {
                    foreach (var termValue in new[] { 1, 2, 3 }) // First, Second, Third
                    {
                        if (term.HasValue && term.Value != termValue)
                            continue;

                        // Convert int term value to Term enum
                        var termEnum = (Term)termValue;

                        var classesQuery = classId.HasValue
                            ? _context.SchoolClasses.Where(c => c.Id == classId.Value)
                            : _context.SchoolClasses;

                        foreach (var schoolClass in await classesQuery.ToListAsync())
                        {
                            // Get student count for this session/term/class combination
                            var studentCount = await _context.TermRegistrations
                                .Where(tr => tr.SessionId == session.Id 
                                    && tr.Term == termEnum 
                                    && tr.SchoolClassId == schoolClass.Id)
                                .CountAsync();

                            if (studentCount == 0)
                                continue;

                            // Get fee setup for this term/class
                            var feeSetup = await _context.TermlyFeesSetups
                                .Where(fs => (Term)fs.Term == termEnum && fs.SchoolClassId == schoolClass.Id)
                                .FirstOrDefaultAsync();

                            if (feeSetup == null)
                                continue;

                            decimal feePerStudent = feeSetup.Amount;
                            decimal expectedAmount = feePerStudent * studentCount;

                            // Get actual paid amount (approved and pending payments that are not cancelled)
                            var actualAmount = await _context.FeesPayments
                                .Where(fp => fp.TermRegistration.SessionId == session.Id && fp.TermRegistration.Term == termEnum && fp.TermRegistration.SchoolClassId == schoolClass.Id && (fp.Status == PaymentStatus.Approved || fp.Status == PaymentStatus.Pending) && fp.PaymentState != PaymentState.Cancelled && fp.Status != PaymentStatus.Rejected)
                                .SumAsync(fp => (decimal?)fp.Amount) ?? 0m;

                            decimal outstandingAmount = expectedAmount - actualAmount;

                            var reportItem = new FeesReportLineItemViewModel
                            {
                                Session = session.Name,
                                TermName = termEnum.ToString(),
                                ClassName = schoolClass.Name,
                                StudentCount = studentCount,
                                FeeAmountPerStudent = feePerStudent,
                                ExpectedAmount = expectedAmount,
                                ActualAmount = actualAmount,
                                OutstandingAmount = outstandingAmount
                            };

                            reportData.Add(reportItem);
                        }
                    }
                }

                // Calculate totals
                reportViewModel.TotalStudents = reportData.Sum(x => x.StudentCount);
                reportViewModel.TotalExpected = reportData.Sum(x => x.ExpectedAmount);
                reportViewModel.TotalActual = reportData.Sum(x => x.ActualAmount);
                reportViewModel.TotalOutstanding = reportData.Sum(x => x.OutstandingAmount);

                reportViewModel.ReportData = reportData.OrderBy(x => x.Session).ThenBy(x => x.TermName).ThenBy(x => x.ClassName).ToList();
                reportViewModel.SelectedSessionId = sessionId;
                reportViewModel.SelectedTerm = term;
                reportViewModel.SelectedClassId = classId;

                return reportViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating fees report: {ex.Message}");
                return new FeesReportViewModel
                {
                    Sessions = new List<SessionDropdownViewModel>(),
                    Terms = new List<TermDropdownViewModel>(),
                    Classes = new List<ClassDropdownViewModel>(),
                    ReportData = new List<FeesReportLineItemViewModel>()
                };
            }
        }

        /// <summary>
        /// Get payment detail with staff information for viewing
        /// </summary>
        public async Task<PaymentDetailViewModel> GetPaymentDetailAsync(int paymentId)
        {
            try
            {
                var payment = await _context.FeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.TermlyFeesSetup)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return null;

                // Get staff user information
                var staffUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == payment.StaffUserId);

                // Get total paid before this payment
                var totalPaidBefore = await _context.FeesPayments
                    .Where(x => x.TermRegId == payment.TermRegId && x.Id < payment.Id && x.PaymentState != PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var detail = new PaymentDetailViewModel
                {
                    PaymentId = payment.Id,
                    InvoiceNumber = payment.InvoiceNumber,
                    StudentName = payment.TermRegistration.Student.FullName,
                    StudentRegNumber = payment.TermRegistration.Student.ApplicationUser.UserName,
                    SessionName = payment.TermRegistration.SessionYear.Name,
                    Term = payment.TermRegistration.Term.ToString(),
                    ClassName = payment.TermRegistration.SchoolClass.Name,
                    AmountPaid = payment.Amount,
                    TotalFees = payment.Fees,
                    TotalPaidBefore = totalPaidBefore,
                    ApprovalStatus = payment.Status,
                    PaymentState = payment.PaymentState,
                    FilePath = payment.FilePath,
                    Message = payment.Message,
                    Notes = payment.Narration,
                    CreatedDate = payment.CreatedDate,
                    UpdatedDate = payment.UpdatedDate,
                    StaffUserId = payment.StaffUserId,
                    StaffName = staffUser?.UserName ?? "Unknown",
                    StaffEmail = staffUser?.Email ?? "N/A",
                    CanApprove = payment.Status == PaymentStatus.Pending,
                    CanReject = payment.Status == PaymentStatus.Pending,
                    CanCancel = payment.Status == PaymentStatus.Rejected && payment.PaymentState != PaymentState.Cancelled
                };

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment detail: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Approve a fees payment
        /// </summary>
        public async Task<ServiceResponse<bool>> ApprovePaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.FeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be approved.");

                payment.Status = PaymentStatus.Approved;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.FeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Payment Approval",
                        entityType: "FeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Payment {payment.InvoiceNumber} has been approved.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Payment approved successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to approve payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while approving the payment.");
            }
        }

        /// <summary>
        /// Reject a fees payment
        /// </summary>
        public async Task<ServiceResponse<bool>> RejectPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.FeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be rejected.");

                payment.Status = PaymentStatus.Rejected;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.FeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Payment Rejection",
                        entityType: "FeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Payment {payment.InvoiceNumber} has been rejected.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Payment rejected successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to reject payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while rejecting the payment.");
            }
        }

        /// <summary>
        /// Cancel a fees payment (must be rejected first)
        /// </summary>
        public async Task<ServiceResponse<bool>> CancelPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.FeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                // Payment must be rejected before canceling
                if (payment.Status != PaymentStatus.Rejected)
                    return ServiceResponse<bool>.Failure("Payment must be rejected before canceling.");

                if (payment.PaymentState == PaymentState.Cancelled)
                    return ServiceResponse<bool>.Failure("Payment is already cancelled.");

                payment.PaymentState = PaymentState.Cancelled;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.FeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Payment Cancellation",
                        entityType: "FeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Payment {payment.InvoiceNumber} has been cancelled.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Payment cancelled successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to cancel payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error canceling payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while canceling the payment.");
            }
        }

    }
}
