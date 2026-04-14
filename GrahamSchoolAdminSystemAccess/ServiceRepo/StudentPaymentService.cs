using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class StudentPaymentService : IStudentPaymentService
    {
        private readonly ApplicationDbContext _context;

        public StudentPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string GetTermName(Term term)
        {
            return term switch
            {
                Term.First => "First Term",
                Term.Second => "Second Term",
                Term.Third => "Third Term",
                _ => "Unknown"
            };
        }

        public async Task<MakePaymentPageViewModel> GetPayableItemsAsync(int termRegistrationId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(t => t.Student).ThenInclude(s => s.ApplicationUser)
                    .Include(t => t.SchoolClass)
                    .Include(t => t.SessionYear)
                    .FirstOrDefaultAsync(t => t.Id == termRegistrationId);

                if (termReg == null) return null;

                // Get payment setups matching this term registration's class, session, and term
                var paymentSetups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem)
                        .ThenInclude(pi => pi.PaymentCategory)
                    .Where(ps => ps.SessionId == termReg.SessionId
                              && ps.Term == termReg.Term
                              && ps.ClassId == termReg.SchoolClassId
                              && ps.IsActive)
                    .Where(ps => ps.PaymentItem.IsActive && ps.PaymentItem.PaymentCategory.IsActive)
                    .ToListAsync();

                // Get already paid amounts for this student in this term registration
                var alreadyPaidAmounts = await _context.StudentPaymentItems
                    .Where(spi => spi.StudentPayment.TermRegId == termRegistrationId
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed
                               && spi.StudentPayment.State != PaymentState.Rejected
                               && spi.StudentPayment.State != PaymentState.Cancelled)
                    .GroupBy(spi => spi.PaymentItemId)
                    .Select(g => new { PaymentItemId = g.Key, TotalPaid = g.Sum(x => x.AmountPaid) })
                    .ToDictionaryAsync(x => x.PaymentItemId, x => x.TotalPaid);

                // Build category groups
                var categoryGroups = paymentSetups
                    .GroupBy(ps => new { ps.PaymentItem.CategoryId, ps.PaymentItem.PaymentCategory.Name })
                    .Select(g => new CategoryGroupViewModel
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        Items = g.Select(ps => new PayableItemViewModel
                        {
                            PaymentItemId = ps.PaymentItemId,
                            ItemName = ps.PaymentItem.Name,
                            CategoryName = g.Key.Name,
                            CategoryId = g.Key.CategoryId,
                            ExpectedAmount = ps.Amount,
                            AlreadyPaid = alreadyPaidAmounts.ContainsKey(ps.PaymentItemId)
                                ? alreadyPaidAmounts[ps.PaymentItemId] : 0
                        }).ToList()
                    })
                    .OrderBy(g => g.CategoryName)
                    .ToList();

                return new MakePaymentPageViewModel
                {
                    TermRegistrationId = termRegistrationId,
                    StudentId = termReg.StudentId,
                    StudentName = termReg.Student?.FullName ?? "Unknown",
                    AdmissionNo = termReg.Student?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = termReg.SchoolClass?.Name ?? "Unknown",
                    SessionName = termReg.SessionYear?.Name ?? "Unknown",
                    TermName = GetTermName(termReg.Term),
                    CategoryGroups = categoryGroups
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payable items: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<MakePaymentPageViewModel>> LookupPayableItemsAsync(string admissionNo, int classId, int categoryId)
        {
            try
            {
                // Step 1: Get current session and term from AppSettings (single-row table)
                var appSettings = await _context.AppSettings
                    .Include(a => a.SessionYear)
                    .FirstOrDefaultAsync();
                if (appSettings == null)
                    return ServiceResponse<MakePaymentPageViewModel>.Failure("Application settings not configured. Please set current session and term in App Settings.");

                int currentSessionId = appSettings.SessionId;
                Term currentTerm = appSettings.Term;
                string sessionName = appSettings.SessionYear?.Name ?? "Unknown";
                string termName = GetTermName(currentTerm);

                // Step 2: Verify student exists by admission number
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == admissionNo);
                if (student == null)
                    return ServiceResponse<MakePaymentPageViewModel>.Failure("Student with this admission number does not exist.");

                // Step 3: Check if student is registered for current Session, Term, and selected Class
                var termReg = await _context.TermRegistrations
                    .Include(t => t.SchoolClass)
                    .Include(t => t.SessionYear)
                    .FirstOrDefaultAsync(t => t.StudentId == student.Id
                                           && t.SessionId == currentSessionId
                                           && t.Term == currentTerm
                                           && t.SchoolClassId == classId);
                if (termReg == null)
                    return ServiceResponse<MakePaymentPageViewModel>.Failure(
                        $"Student '{student.FullName}' is not registered for {termName} in {sessionName} for the selected class.");

                // Step 4: Check if payment setup exists for the selected category in this class/session/term
                var paymentSetups = await _context.PaymentSetups
                    .Include(ps => ps.PaymentItem)
                        .ThenInclude(pi => pi.PaymentCategory)
                    .Where(ps => ps.SessionId == currentSessionId
                              && ps.Term == currentTerm
                              && ps.ClassId == classId
                              && ps.IsActive
                              && ps.PaymentItem.IsActive
                              && ps.PaymentItem.PaymentCategory.IsActive
                              && ps.PaymentItem.CategoryId == categoryId)
                    .ToListAsync();

                if (!paymentSetups.Any())
                    return ServiceResponse<MakePaymentPageViewModel>.Failure(
                        "No payment items are configured for the selected category in this class/session/term.");

                // Step 5: Get already paid amounts for this student in this term registration
                var alreadyPaidAmounts = await _context.StudentPaymentItems
                    .Where(spi => spi.StudentPayment.TermRegId == termReg.Id
                               && spi.StudentPayment.Status != PaymentStatus.Reversed
                               && spi.StudentPayment.Status != PaymentStatus.Failed
                               && spi.StudentPayment.State != PaymentState.Rejected
                               && spi.StudentPayment.State != PaymentState.Cancelled)
                    .GroupBy(spi => spi.PaymentItemId)
                    .Select(g => new { PaymentItemId = g.Key, TotalPaid = g.Sum(x => x.AmountPaid) })
                    .ToDictionaryAsync(x => x.PaymentItemId, x => x.TotalPaid);

                // Step 6: Build category groups
                var categoryGroups = paymentSetups
                    .GroupBy(ps => new { ps.PaymentItem.CategoryId, ps.PaymentItem.PaymentCategory.Name })
                    .Select(g => new CategoryGroupViewModel
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.Name,
                        Items = g.Select(ps => new PayableItemViewModel
                        {
                            PaymentItemId = ps.PaymentItemId,
                            ItemName = ps.PaymentItem.Name,
                            CategoryName = g.Key.Name,
                            CategoryId = g.Key.CategoryId,
                            ExpectedAmount = ps.Amount,
                            AlreadyPaid = alreadyPaidAmounts.ContainsKey(ps.PaymentItemId)
                                ? alreadyPaidAmounts[ps.PaymentItemId] : 0
                        }).ToList()
                    })
                    .OrderBy(g => g.CategoryName)
                    .ToList();

                var result = new MakePaymentPageViewModel
                {
                    TermRegistrationId = termReg.Id,
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    AdmissionNo = student.ApplicationUser?.UserName ?? admissionNo,
                    ClassName = termReg.SchoolClass?.Name ?? "Unknown",
                    SessionName = sessionName,
                    TermName = termName,
                    CategoryGroups = categoryGroups
                };

                return ServiceResponse<MakePaymentPageViewModel>.Success(result, "Student found and payment items loaded.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<MakePaymentPageViewModel>.Failure($"Error looking up payable items: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<int>> CreatePaymentAsync(CreatePaymentViewModel model, string? evidenceFilePath = null)
        {
            try
            {
                if (model.Items == null || !model.Items.Any())
                    return ServiceResponse<int>.Failure("No payment items selected");

                // Remove items with zero or negative amounts
                model.Items = model.Items.Where(i => i.AmountPaid > 0).ToList();
                if (!model.Items.Any())
                    return ServiceResponse<int>.Failure("All selected items have zero amounts");

                // Server-side evidence check from AppSettings
                var appSettings = await _context.AppSettings.FirstOrDefaultAsync();
                if (appSettings != null && appSettings.PaymentEvidence && string.IsNullOrEmpty(evidenceFilePath))
                    return ServiceResponse<int>.Failure("Payment evidence is required. Please upload evidence of payment.");

                var termReg = await _context.TermRegistrations
                    .FirstOrDefaultAsync(t => t.Id == model.TermRegistrationId);
                if (termReg == null)
                    return ServiceResponse<int>.Failure("Term registration not found");

                // Overpayment prevention
                foreach (var item in model.Items)
                {
                    var expected = await _context.PaymentSetups
                        .Where(ps => ps.PaymentItemId == item.PaymentItemId
                                  && ps.SessionId == termReg.SessionId
                                  && ps.Term == termReg.Term
                                  && ps.ClassId == termReg.SchoolClassId)
                        .Select(ps => ps.Amount)
                        .FirstOrDefaultAsync();

                    if (expected == 0)
                        return ServiceResponse<int>.Failure($"Payment item {item.PaymentItemId} is not configured for this class/session/term");

                    var alreadyPaid = await _context.StudentPaymentItems
                        .Where(spi => spi.PaymentItemId == item.PaymentItemId
                                   && spi.StudentPayment.TermRegId == model.TermRegistrationId
                                   && spi.StudentPayment.Status != PaymentStatus.Reversed
                                   && spi.StudentPayment.Status != PaymentStatus.Failed
                                   && spi.StudentPayment.State != PaymentState.Rejected
                                   && spi.StudentPayment.State != PaymentState.Cancelled)
                        .SumAsync(spi => (decimal?)spi.AmountPaid) ?? 0;

                    if ((alreadyPaid + item.AmountPaid) > expected)
                        return ServiceResponse<int>.Failure($"Overpayment detected for item. Expected: {expected}, Already Paid: {alreadyPaid}, Attempting: {item.AmountPaid}");
                }

                var payment = new StudentPayment
                {
                    TermRegId = model.TermRegistrationId,
                    TotalAmount = model.Items.Sum(i => i.AmountPaid),
                    PaymentDate = DateTime.UtcNow,
                    Reference = SD.GenerateUniqueNumber(),
                    Status = PaymentStatus.Completed,
                    State = PaymentState.Pending,
                    Narration = model.Narration,
                    EvidenceFilePath = evidenceFilePath,
                    PaymentItems = model.Items.Select(i => new StudentPaymentItem
                    {
                        PaymentItemId = i.PaymentItemId,
                        AmountPaid = i.AmountPaid
                    }).ToList()
                };

                _context.StudentPayments.Add(payment);
                await _context.SaveChangesAsync();

                return ServiceResponse<int>.Success(payment.Id, "Payment recorded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.Failure($"Error creating payment: {ex.Message}");
            }
        }

        public async Task<PaymentReceiptViewModel> GetReceiptAsync(int paymentId)
        {
            try
            {
                var payment = await _context.StudentPayments
                    .Include(p => p.TermRegistration).ThenInclude(t => t.Student).ThenInclude(s => s.ApplicationUser)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SchoolClass)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SessionYear)
                    .Include(p => p.PaymentItems).ThenInclude(pi => pi.PaymentItem).ThenInclude(i => i.PaymentCategory)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null) return null;

                return new PaymentReceiptViewModel
                {
                    PaymentId = payment.Id,
                    TermRegId = payment.TermRegId,
                    Reference = payment.Reference,
                    StudentName = payment.TermRegistration?.Student?.FullName ?? "Unknown",
                    AdmissionNo = payment.TermRegistration?.Student?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = payment.TermRegistration?.SchoolClass?.Name ?? "Unknown",
                    SessionName = payment.TermRegistration?.SessionYear?.Name ?? "Unknown",
                    TermName = GetTermName(payment.TermRegistration?.Term ?? Term.First),
                    PaymentDate = payment.PaymentDate,
                    TotalAmount = payment.TotalAmount,
                    Status = payment.Status.ToString(),
                    State = payment.State.ToString(),
                    Narration = payment.Narration,
                    RejectMessage = payment.RejectMessage,
                    EvidenceFilePath = payment.EvidenceFilePath,
                    LineItems = payment.PaymentItems?.Select(pi => new ReceiptLineItem
                    {
                        CategoryName = pi.PaymentItem?.PaymentCategory?.Name ?? "Unknown",
                        ItemName = pi.PaymentItem?.Name ?? "Unknown",
                        AmountPaid = pi.AmountPaid
                    }).OrderBy(li => li.CategoryName).ThenBy(li => li.ItemName).ToList() ?? new()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving receipt: {ex.Message}", ex);
            }
        }

        public async Task<ConsolidatedReceiptViewModel> GetConsolidatedReceiptAsync(int termRegId)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(t => t.Student).ThenInclude(s => s.ApplicationUser)
                    .Include(t => t.SchoolClass)
                    .Include(t => t.SessionYear)
                    .FirstOrDefaultAsync(t => t.Id == termRegId);

                if (termReg == null) return null;

                var paymentItems = await _context.StudentPaymentItems
                    .Include(spi => spi.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(spi => spi.StudentPayment)
                    .Where(spi => spi.StudentPayment.TermRegId == termRegId
                                && spi.StudentPayment.Status != PaymentStatus.Reversed
                                && spi.StudentPayment.Status != PaymentStatus.Failed
                                && spi.StudentPayment.State != PaymentState.Rejected
                                && spi.StudentPayment.State != PaymentState.Cancelled)
                    .ToListAsync();

                var categories = paymentItems
                    .GroupBy(spi => spi.PaymentItem?.PaymentCategory?.Name ?? "Unknown")
                    .Select(g => new ReceiptCategoryGroup
                    {
                        CategoryName = g.Key,
                        PaymentReferences = g.Select(spi => spi.StudentPayment?.Reference)
                                             .Where(r => !string.IsNullOrEmpty(r))
                                             .Distinct()
                                             .OrderBy(r => r)
                                             .ToList(),
                        Items = g.GroupBy(spi => new { Name = spi.PaymentItem?.Name ?? "Unknown", spi.PaymentItemId })
                                  .Select(ig => new ReceiptLineItem
                                  {
                                      CategoryName = g.Key,
                                      ItemName = ig.Key.Name,
                                      AmountPaid = ig.Sum(x => x.AmountPaid)
                                  })
                                  .OrderBy(i => i.ItemName)
                                  .ToList()
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                return new ConsolidatedReceiptViewModel
                {
                    TermRegistrationId = termRegId,
                    StudentName = termReg.Student?.FullName ?? "Unknown",
                    AdmissionNo = termReg.Student?.ApplicationUser?.UserName ?? "N/A",
                    ClassName = termReg.SchoolClass?.Name ?? "Unknown",
                    SessionName = termReg.SessionYear?.Name ?? "Unknown",
                    TermName = GetTermName(termReg.Term),
                    PrintDate = DateTime.UtcNow,
                    Categories = categories
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving consolidated receipt: {ex.Message}", ex);
            }
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentsDataTableAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null,
            string? statusFilter = null, int? stateFilter = null)
        {
            try
            {
                var query = _context.StudentPayments
                    .Include(p => p.TermRegistration).ThenInclude(t => t.Student).ThenInclude(s => s.ApplicationUser)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SchoolClass)
                    .Include(p => p.TermRegistration).ThenInclude(t => t.SessionYear)
                    .AsQueryable();

                if (sessionFilter.HasValue && sessionFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.SessionId == sessionFilter.Value);
                if (termFilter.HasValue && termFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.Term == (Term)termFilter.Value);
                if (classFilter.HasValue && classFilter.Value > 0)
                    query = query.Where(p => p.TermRegistration.SchoolClassId == classFilter.Value);
                if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<PaymentStatus>(statusFilter, true, out var parsedStatus))
                    query = query.Where(p => p.Status == parsedStatus);
                if (stateFilter.HasValue && stateFilter.Value > 0 && Enum.IsDefined(typeof(PaymentState), stateFilter.Value))
                    query = query.Where(p => p.State == (PaymentState)stateFilter.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p =>
                        p.Reference.Contains(searchTerm) ||
                        p.TermRegistration.Student.Surname.Contains(searchTerm) ||
                        p.TermRegistration.Student.Firstname.Contains(searchTerm) ||
                        p.TermRegistration.Student.Othername.Contains(searchTerm) ||
                        p.TermRegistration.Student.ApplicationUser.UserName.Contains(searchTerm));
                }

                int recordsTotal = await _context.StudentPayments.CountAsync();
                int recordsFiltered = await query.CountAsync();

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.PaymentDate)
                    : query.OrderBy(p => p.PaymentDate);

                var rawData = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.TermRegId,
                        StudentName = p.TermRegistration.Student.Surname + " " + p.TermRegistration.Student.Firstname,
                        AdmissionNo = p.TermRegistration.Student.ApplicationUser.UserName ?? "N/A",
                        ClassName = p.TermRegistration.SchoolClass.Name,
                        SessionName = p.TermRegistration.SessionYear.Name,
                        p.TermRegistration.Term,
                        p.TotalAmount,
                        p.Reference,
                        p.Status,
                        p.State,
                        PaymentDate = p.PaymentDate.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                var data = rawData.Select(p => new
                {
                    p.Id,
                    p.TermRegId,
                    p.StudentName,
                    p.AdmissionNo,
                    p.ClassName,
                    p.SessionName,
                    TermName = GetTermName(p.Term),
                    TotalAmount = SD.ToNaira(p.TotalAmount),
                    p.Reference,
                    Status = p.Status.ToString(),
                    State = p.State.ToString(),
                    p.PaymentDate
                }).ToList();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving student payments: {ex.Message}", ex);
            }
        }

        public async Task<PaymentReceiptViewModel> GetPaymentDetailAsync(int paymentId)
        {
            return await GetReceiptAsync(paymentId);
        }

        public async Task<ServiceResponse<bool>> UpdatePaymentStateAsync(int paymentId, PaymentState state, string? rejectMessage)
        {
            try
            {
                var payment = await _context.StudentPayments.FindAsync(paymentId);
                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found");

                if (state == PaymentState.Rejected && string.IsNullOrWhiteSpace(rejectMessage))
                    return ServiceResponse<bool>.Failure("A rejection message is required when rejecting a payment.");

                payment.State = state;
                payment.RejectMessage = state == PaymentState.Rejected ? rejectMessage : payment.RejectMessage;
                if(state==PaymentState.Rejected)
                    payment.Status = PaymentStatus.Reversed;

                await _context.SaveChangesAsync();

                return ServiceResponse<bool>.Success(true, $"Payment state updated to {state}");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error updating payment state: {ex.Message}");
            }
        }

        public async Task<List<PendingPaymentNotification>> GetPendingPaymentNotificationsAsync(int maxCount = 20)
        {
            var now = DateTime.UtcNow;

            var pending = await _context.StudentPayments
                .Include(p => p.TermRegistration)
                    .ThenInclude(tr => tr.Student)
                .Include(p => p.TermRegistration)
                    .ThenInclude(tr => tr.SchoolClass)
                .Where(p => p.State == PaymentState.Pending && p.Status != PaymentStatus.Failed && p.Status != PaymentStatus.Reversed)
                .OrderByDescending(p => p.PaymentDate)
                .Take(maxCount)
                .Select(p => new PendingPaymentNotification
                {
                    PaymentId = p.Id,
                    Reference = p.Reference,
                    StudentName = p.TermRegistration.Student.Surname + " " + p.TermRegistration.Student.Firstname,
                    ClassName = p.TermRegistration.SchoolClass.Name,
                    Amount = p.TotalAmount,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            foreach (var item in pending)
            {
                item.TimeAgo = GetTimeAgo(now, item.PaymentDate);
            }

            return pending;
        }

        private static string GetTimeAgo(DateTime now, DateTime date)
        {
            var diff = now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return date.ToString("MMM dd, yyyy");
        }
    }
}
