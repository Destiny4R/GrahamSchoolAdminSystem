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
    public class PTAPaymentServices : IPTAPaymentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PTAPaymentServices> _logger;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PTAPaymentServices(ApplicationDbContext context, ILogger<PTAPaymentServices> logger, ILogService logService, IHttpContextAccessor httpContextAccessor)
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

        /// <summary>
        /// Create a new PTA payment record
        /// </summary>
        public async Task<ServiceResponse<int>> CreatePTAPaymentAsync(RecordPTAPaymentViewModel record)
        {
            try
            {
                // 1. Basic validation
                if (record.amount < 100)
                    return ServiceResponse<int>.Failure("The PTA payment amount is too low to be processed.");

                // 2. Fetch required data
                var ptaFeesSetup = await _context.PTAFeesSetups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == record.PtaFeesSetUpid);

                if (ptaFeesSetup == null)
                    return ServiceResponse<int>.Failure("PTA fees have not yet been configured for the selected class.");

                var studentReg = await _context.TermRegistrations
                    .Include(x => x.Student.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == record.termregid);

                if (studentReg == null)
                    return ServiceResponse<int>.Failure("No student was found with the provided registration number.");

                // 3. Get total previous PTA payments (optimized)
                var totalPaidBefore = await _context.PTAFeesPayments
                    .Where(x => x.TermRegId == record.termregid && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var newTotal = totalPaidBefore + record.amount;

                // 4. Prevent overpayment
                // Rule 1: Single payment cannot exceed total PTA fees
                if (record.amount > ptaFeesSetup.Amount)
                {
                    return ServiceResponse<int>.Failure("Payment amount cannot exceed total PTA fees.");
                }

                // Rule 2: Cumulative payment cannot exceed total PTA fees
                if (newTotal > ptaFeesSetup.Amount)
                {
                    var remainingBalance = ptaFeesSetup.Amount - totalPaidBefore;
                    return ServiceResponse<int>.Failure(
                        $"Payment exceeds remaining balance. Remaining balance is {remainingBalance:N2}."
                    );
                }

                // 5. Handle file upload
                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "feesFiles");

                var fileUploadResult = FileUploadHandler.ProcessFile(record.evidencefile, uploadDirectory);

                if (!fileUploadResult.Success)
                    return ServiceResponse<int>.Failure(fileUploadResult.Message);

                // 6. Create payment entity
                var payment = new PTAFeesPayments
                {
                    Amount = record.amount,
                    PtaFeesSetupId = record.PtaFeesSetUpid,
                    TermRegId = record.termregid,
                    Fees = ptaFeesSetup.Amount,
                    FilePath = $"feesFiles/{fileUploadResult.FileName}",
                    InvoiceNumber = $"PTA-{SD.GenerateUniqueNumber()}",
                    StaffUserId = GetCurrentUserId(),
                    Narration = record.narration ?? string.Empty,
                    Message = string.Empty
                };

                var ifPaybefore = _context.PTAFeesPayments.Include(p => p.TermRegistration).Where(k => k.TermRegId == record.termregid && k.PaymentState != GetEnums.PaymentState.Cancelled && k.Status != PaymentStatus.Rejected).ToList();
                if (ifPaybefore.Any())
                {
                    var totalamount = ifPaybefore.Sum(k => k.Amount);
                    var finalamount = totalamount + record.amount;

                    if (finalamount == ptaFeesSetup.Amount)
                    {
                        payment.PaymentState = PaymentState.Completed;
                    }
                    else
                        payment.PaymentState = PaymentState.PartPayment;
                }
                if (record.amount == ptaFeesSetup.Amount)
                    payment.PaymentState = PaymentState.Completed;
                else
                    payment.PaymentState = PaymentState.PartPayment;


                _context.PTAFeesPayments.Add(payment);

                // 7. Save changes
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    return ServiceResponse<int>.Failure("An error occurred while recording the PTA payment. Please try again.");

                // 8. Log activity
                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "PTA Payment",
                    entityType: "PTAFeesPayment",
                    entityId: payment.Id.ToString(),
                    message: "PTA payment recorded successfully and is pending approval.",
                    ipAddress: GetClientIpAddress(),
                    details: $"Student Reg Number: {record.regnumber}, Session ID: {record.session}, Term: {record.term}"
                );

                return ServiceResponse<int>.Success(payment.Id, "PTA payment recorded successfully and is pending approval.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PTA payment");
                return ServiceResponse<int>.Failure("An unexpected error occurred while processing the PTA payment.");
            }
        }

        /// <summary>
        /// Get paginated list of PTA payments with filtering and sorting
        /// </summary>
        public async Task<(List<PTAPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetPTAPaymentsAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc",
            int? termFilter = null,
            int? sessionFilter = null,
            int? classFilter = null,
            int? subclassFilter = null)
        {
            try
            {
                var query = _context.PTAFeesPayments
                    .Include(tr => tr.TermRegistration.Student.ApplicationUser)
                    .Include(tr => tr.TermRegistration.SchoolClass)
                    .Include(tr => tr.TermRegistration.SessionYear)
                    .Include(tr => tr.PTAFeesSetup)
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
                    .Select(x => new PTAPaymentsDto
                    {
                        id = x.Id,
                        name = $"{x.TermRegistration.Student.FullName}",
                        term = x.TermRegistration.Term.ToString(),
                        session = x.TermRegistration.SessionYear.Name,
                        schoolclass = x.TermRegistration.SchoolClass.Name,
                        createdate = x.TermRegistration.CreatedDate,
                        regnumber = x.TermRegistration.Student.ApplicationUser.UserName,
                        fees = SD.ToNaira(x.PTAFeesSetup.Amount),
                        amount = SD.ToNaira(x.Amount),
                        balance = SD.ToNaira((x.PTAFeesSetup.Amount - x.Amount)),
                        state = SD.GetSpanBadgeState(x.PaymentState),
                        //Get value of Enum and convert value to enum name
                        status = SD.GetSpanBadgeStatus(x.Status)
                    })
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting PTA payments: {ex.Message}");
                return (new List<PTAPaymentsDto>(), 0, 0);
            }
        }

        /// <summary>
        /// Search for PTA payment information
        /// </summary>
        public async Task<(RecordPTAPaymentViewModel Data, bool Succeeded, string Message)> SearchPTAPaymentAsync(RecordPTAPaymentSearchViewModel model)
        {
            try
            {
                var ptaFeesSetup = await _context.PTAFeesSetups
                    .FirstOrDefaultAsync(k => (int)k.Term == model.term && k.SessionId == model.sessionid && k.SchoolClassId == model.schoolclass);

                if (ptaFeesSetup == null)
                {
                    return (null, false, "PTA fees have not yet been configured for the selected class.");
                }

                var studentInfo = await _context.TermRegistrations
                    .Include(k => k.Student.ApplicationUser)
                    .Include(k => k.SessionYear)
                    .Include(k => k.SchoolClass)
                    .Include(k => k.SchoolSubClass)
                    .FirstOrDefaultAsync(k => k.Student.ApplicationUser.UserName == model.StudentRegNumber && (int)k.Term == model.term && k.SessionId == model.sessionid && k.SchoolClassId == model.schoolclass);

                if (studentInfo == null)
                {
                    return (null, false, "No student was found with the provided registration number.");
                }

                RecordPTAPaymentViewModel recordPaymentViewModel = new()
                {
                    term = ptaFeesSetup.Term,
                    termregid = studentInfo.Id,
                    PtaFeesSetUpid = ptaFeesSetup.Id,
                    name = studentInfo.Student.FullName,
                    session = studentInfo.SessionYear.Name,
                    schoolclass = $"{studentInfo.SchoolClass.Name} - {studentInfo.SchoolSubClass.Name}",
                    regnumber = studentInfo.Student.ApplicationUser.UserName,
                    ptafees = ptaFeesSetup.Amount,
                };

                // Check if there are previous PTA payments
                var previousPayments = _context.PTAFeesPayments
                    .Where(k => k.TermRegId == studentInfo.Id && k.PaymentState != GetEnums.PaymentState.Cancelled && k.Status != PaymentStatus.Rejected)
                    .ToList();

                if (previousPayments.Any())
                {
                    recordPaymentViewModel.balance = previousPayments.Sum(k => k.Amount);
                }

                return (recordPaymentViewModel, true, "Retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching PTA payment: {ex.Message}");
                return (null, false, "An error occurred while searching for PTA payment information.");
            }
        }

        /// <summary>
        /// Get total previous PTA payments for a student
        /// </summary>
        public async Task<decimal> GetTotalPreviousPTAPaymentsAsync(int termRegId, int excludePaymentId = 0)
        {
            try
            {
                var total = await _context.PTAFeesPayments
                    .Where(x => x.TermRegId == termRegId && x.Id != excludePaymentId && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total previous PTA payments: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get previous balance (outstanding amount) for a student term registration
        /// </summary>
        public async Task<decimal> GetPreviousBalanceAsync(int termRegId)
        {
            try
            {
                var termRegistration = await _context.TermRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == termRegId);

                if (termRegistration == null)
                    return 0;

                var ptaFeesSetup = await _context.PTAFeesSetups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Term == termRegistration.Term && x.SchoolClassId == termRegistration.SchoolClassId && x.SessionId == termRegistration.SessionId);

                if (ptaFeesSetup == null)
                    return 0;

                var totalPaid = await _context.PTAFeesPayments
                    .Where(x => x.TermRegId == termRegId && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                return ptaFeesSetup.Amount - totalPaid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting previous balance: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get PTA fees report data
        /// </summary>
        public async Task<PTAFeesReportViewModel> GetPTAFeesReportAsync(int? sessionId = null, int? term = null, int? classId = null)
        {
            try
            {
                var reportViewModel = new PTAFeesReportViewModel();

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

                var reportData = new List<PTAFeesReportLineItemViewModel>();

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

                            // Get PTA fee setup for this term/class
                            var feeSetup = await _context.PTAFeesSetups
                                .Where(fs => (Term)fs.Term == termEnum && fs.SchoolClassId == schoolClass.Id && fs.SessionId == session.Id)
                                .FirstOrDefaultAsync();

                            if (feeSetup == null)
                                continue;

                            decimal feePerStudent = feeSetup.Amount;
                            decimal expectedAmount = feePerStudent * studentCount;

                            // Get actual paid amount (confirmed PTA payments)
                            var actualAmount = await _context.PTAFeesPayments
                                .Where(fp => fp.TermRegistration.SessionId == session.Id && fp.TermRegistration.Term == termEnum && fp.TermRegistration.SchoolClassId == schoolClass.Id && fp.PaymentState != GetEnums.PaymentState.Cancelled && fp.Status != PaymentStatus.Rejected)
                                .SumAsync(fp => (decimal?)fp.Amount) ?? 0m;

                            decimal outstandingAmount = expectedAmount - actualAmount;

                            // Calculate collection percentage for this line item
                            decimal collectionPercentage = expectedAmount > 0 
                                ? Math.Round((actualAmount / expectedAmount) * 100, 2) 
                                : 0;

                            var reportItem = new PTAFeesReportLineItemViewModel
                            {
                                Session = session.Name,
                                TermName = termEnum.ToString(),
                                ClassName = schoolClass.Name,
                                StudentCount = studentCount,
                                FeeAmountPerStudent = feePerStudent,
                                ExpectedAmount = expectedAmount,
                                ActualAmount = actualAmount,
                                OutstandingAmount = outstandingAmount,
                                CollectionPercentage = collectionPercentage
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

                // Calculate overall collection percentage
                reportViewModel.OverallCollectionPercentage = reportViewModel.TotalExpected > 0
                    ? Math.Round((reportViewModel.TotalActual / reportViewModel.TotalExpected) * 100, 2)
                    : 0;

                reportViewModel.ReportData = reportData.OrderBy(x => x.Session).ThenBy(x => x.TermName).ThenBy(x => x.ClassName).ToList();
                reportViewModel.SelectedSessionId = sessionId;
                reportViewModel.SelectedTerm = term;
                reportViewModel.SelectedClassId = classId;

                return reportViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating PTA fees report: {ex.Message}");
                return new PTAFeesReportViewModel
                {
                    Sessions = new List<SessionDropdownViewModel>(),
                    Terms = new List<TermDropdownViewModel>(),
                    Classes = new List<ClassDropdownViewModel>(),
                    ReportData = new List<PTAFeesReportLineItemViewModel>()
                };
            }
        }

        /// <summary>
        /// Get PTA payment detail with staff information
        /// </summary>
        public async Task<PTAPaymentDetailViewModel> GetPTAPaymentDetailAsync(int paymentId)
        {
            try
            {
                var payment = await _context.PTAFeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.PTAFeesSetup)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return null;

                // Get staff user information
                var staffUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == payment.StaffUserId);

                // Get total paid before this payment
                var totalPaidBefore = await _context.PTAFeesPayments
                    .Where(x => x.TermRegId == payment.TermRegId && x.Id < payment.Id && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var detail = new PTAPaymentDetailViewModel
                {
                    PaymentId = payment.Id,
                    InvoiceNumber = payment.InvoiceNumber,
                    StudentName = payment.TermRegistration.Student.FullName,
                    StudentRegNumber = payment.TermRegistration.Student.ApplicationUser.UserName,
                    AcademicSession = payment.TermRegistration.SessionYear.Name,
                    Term = payment.TermRegistration.Term.ToString(),
                    SchoolClass = payment.TermRegistration.SchoolClass.Name,
                    Amount = payment.Amount,
                    TotalPTAFees = payment.Fees,
                    TotalPaidBefore = totalPaidBefore,
                    FilePath = payment.FilePath,
                    Message = payment.Message,
                    Narration = payment.Narration,
                    CreatedDate = payment.CreatedDate,
                    UpdatedDate = payment.UpdatedDate,
                    StaffUserId = payment.StaffUserId,
                    PaymentState = payment.PaymentState,
                    Status = payment.Status,
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
                _logger.LogError($"Error getting PTA payment detail: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Approve a PTA payment
        /// </summary>
        public async Task<ServiceResponse<bool>> ApprovePTAPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.PTAFeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be approved.");

                payment.Status = PaymentStatus.Approved;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.PTAFeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "PTA Payment Approval",
                        entityType: "PTAFeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"PTA Payment {payment.InvoiceNumber} has been approved.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "PTA payment approved successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to approve PTA payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving PTA payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while approving the PTA payment.");
            }
        }

        /// <summary>
        /// Reject a PTA payment
        /// </summary>
        public async Task<ServiceResponse<bool>> RejectPTAPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.PTAFeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be rejected.");

                payment.Status = PaymentStatus.Rejected;
                payment.UpdatedDate = DateTime.UtcNow;

                // Delete or mark the payment as rejected (in this case, we delete it)
                _context.PTAFeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "PTA Payment Rejection",
                        entityType: "PTAFeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"PTA Payment {payment.InvoiceNumber} has been rejected and removed.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "PTA payment rejected successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to reject PTA payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting PTA payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while rejecting the PTA payment.");
            }
        }

        /// <summary>
        /// Cancel a PTA payment
        /// </summary>
        public async Task<ServiceResponse<bool>> CancelPTAPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.PTAFeesPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                // Payment must be rejected before canceling
                if (payment.Status != PaymentStatus.Rejected)
                    return ServiceResponse<bool>.Failure("Payment must be rejected before canceling.");

                if (payment.PaymentState == PaymentState.Cancelled)
                    return ServiceResponse<bool>.Failure("Payment is already cancelled.");

                payment.PaymentState = PaymentState.Cancelled;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.PTAFeesPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "PTA Payment Cancellation",
                        entityType: "PTAFeesPayment",
                        entityId: payment.Id.ToString(),
                        message: $"PTA Payment {payment.InvoiceNumber} has been cancelled.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "PTA payment cancelled successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to cancel PTA payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error canceling PTA payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while canceling the PTA payment.");
            }
        }

        /// <summary>
        /// Get PTA payment by ID for invoice generation
        /// </summary>
        public async Task<PTAFeesPayments> GetPTAPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _context.PTAFeesPayments
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.TermRegistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.PTAFeesSetup)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting PTA payment by ID: {ex.Message}");
                return null;
            }
        }
    }
}
