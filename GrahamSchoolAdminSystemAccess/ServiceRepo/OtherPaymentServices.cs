using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class OtherPaymentServices : IOtherPaymentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OtherPaymentServices> _logger;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OtherPaymentServices(ApplicationDbContext context, ILogger<OtherPaymentServices> logger, ILogService logService, IHttpContextAccessor httpContextAccessor)
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

        public async Task<(bool Succeeded, string Message, object Data)> CreateAndUpdateItemAsync(OtherPayItemsViewModel model)
        {
            try
            {
                // Update existing item
                if (model.Id.HasValue && model.Id > 0)
                {
                    var existingItem = await _context.OtherPayItemsTable
                        .FirstOrDefaultAsync(x => x.Id == model.Id);

                    if (existingItem == null)
                    {
                        return (false, "Payment item not found", null);
                    }

                    // Check if name is already used by another item
                    var nameExists = await _context.OtherPayItemsTable
                        .AnyAsync(x => x.Name == model.name && x.Id != model.Id);

                    if (nameExists)
                    {
                        return (false, "A payment item with this name already exists", null);
                    }

                    var oldName = existingItem.Name;
                    var oldDescription = existingItem.Description;

                    existingItem.Name = model.name.Trim();
                    existingItem.Description = model.description?.Trim() ?? string.Empty;

                    _context.OtherPayItemsTable.Update(existingItem);
                    await _context.SaveChangesAsync();

                    // Log the action using ILogService
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Item Update",
                        entityType: "OtherPayItem",
                        entityId: existingItem.Id.ToString(),
                        message: "Other payment item updated successfully.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Item Name: {existingItem.Name}, Description: {existingItem.Description}, Previous Name: {oldName}"
                    );

                    return (true, "Payment item updated successfully", existingItem);
                }
                else
                {
                    // Create new item
                    var nameExists = await _context.OtherPayItemsTable
                        .AnyAsync(x => x.Name == model.name);

                    if (nameExists)
                    {
                        return (false, "A payment item with this name already exists", null);
                    }

                    var newItem = new OtherPayItemsTable
                    {
                        Name = model.name.Trim(),
                        Description = model.description?.Trim() ?? string.Empty,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.OtherPayItemsTable.Add(newItem);
                    await _context.SaveChangesAsync();

                    // Log the action using ILogService
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Item Creation",
                        entityType: "OtherPayItem",
                        entityId: newItem.Id.ToString(),
                        message: "Other payment item created successfully.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Item Name: {newItem.Name}, Description: {newItem.Description}"
                    );

                    return (true, "Payment item created successfully", newItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating payment item with name: {ItemName} by user {UserName}", model.name, GetCurrentUserName());
                return (false, "An unexpected error occurred", null);
            }
        }

        public async Task<(bool Succeeded, string Message)> DeleteOtherItemAsync(int id)
        {
            try
            {
                if(await _context.OtherPayFeesSetUp.AsNoTracking().Where(x => x.OtherPayId == id).AnyAsync())
                {
                    _logger.LogWarning("Attempted to delete payment item {ItemId} but it is associated with payment fee setups by user {UserName}", id, GetCurrentUserName());

                    // Log the failed action using ILogService
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Item Deletion Attempt",
                        entityType: "OtherPayItem",
                        entityId: id.ToString(),
                        message: "Failed - Item is associated with payment fee setups.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Item ID: {id}, Reason: Cannot delete payment item because it is associated with other pay setups"
                    );

                    return (false, "Cannot delete payment item because it is associated with other pay setups");
                }

                var item = await _context.OtherPayItemsTable.FirstOrDefaultAsync(x => x.Id == id);

                if (item == null)
                {
                    return (false, "Payment item not found");
                }

                var itemName = item.Name;
                _context.OtherPayItemsTable.Remove(item);
                await _context.SaveChangesAsync();


                // Log the action using ILogService
                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Other Payment Item Deletion",
                    entityType: "OtherPayItem",
                    entityId: id.ToString(),
                    message: "Other payment item deleted successfully.",
                    ipAddress: GetClientIpAddress(),
                    details: $"Item ID: {id}, Item Name: {itemName}"
                );

                return (true, "Payment item deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment item {ItemId} by user {UserName}", id, GetCurrentUserName());
                return (false, "An unexpected error occurred while deleting the payment item");
            }
        }

        public async Task<OtherPayItemsTable> GetOtherItemByIdAsync(int id)
        {
            try
            {
                var item = await _context.OtherPayItemsTable.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment item by id {ItemId}", id);
                return null;
            }
        }

        public async Task<(List<OtherPayItemsDtos> data, int recordsTotal, int recordsFiltered)> GetOtherItemsListAsync(int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.OtherPayItemsTable
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(x =>
                        x.Name.Contains(searchValue) ||
                        x.Description.Contains(searchValue));
                }

                var recordsFiltered = await query.CountAsync();

                // Apply sorting
                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                    2 => sortDirection == "asc" ? query.OrderBy(x => x.CreatedDate) : query.OrderByDescending(x => x.CreatedDate),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var items = await query.Skip(start).Take(length).ToListAsync();

                var data = items.Select(x => new OtherPayItemsDtos
                {
                    id = x.Id,
                    name = x.Name,
                    description = x.Description,
                    createdate = x.CreatedDate
                }).ToList();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment items list");
                return (new List<OtherPayItemsDtos>(), 0, 0);
            }
        }

        public IEnumerable<SelectListItem> OtherPaymentItemsOptions()
        {
            try
            {
                var items = _context.OtherPayItemsTable
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToList();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting other payment items options");
                return new List<SelectListItem>();
            }
        }

        // ================= Other Payment lifecycle methods =================
        public async Task<ServiceResponse<int>> CreateOtherPaymentAsync(RecordPaymentViewModel record)
        {
            try
            {
                if (record.amount < 100)
                    return ServiceResponse<int>.Failure("The other payment amount is too low to be processed.");

                var feeSetup = await _context.OtherPayFeesSetUp
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == record.FeesSetUpid);

                if (feeSetup == null)
                    return ServiceResponse<int>.Failure("Other payment fees have not yet been configured for the selected class.");

                var studentReg = await _context.TermRegistrations
                    .Include(x => x.Student.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == record.termregid);

                if (studentReg == null)
                    return ServiceResponse<int>.Failure("No student was found with the provided registration number.");

                var totalPaidBefore = await _context.OtherPayments
                    .Where(x => x.TermRegId == record.termregid && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != GetEnums.PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var newTotal = totalPaidBefore + record.amount;

                if (record.amount > (decimal)feeSetup.Amount)
                {
                    return ServiceResponse<int>.Failure("Payment amount cannot exceed configured item amount.");
                }

                if (newTotal > (decimal)feeSetup.Amount)
                {
                    var remainingBalance = (decimal)feeSetup.Amount - totalPaidBefore;
                    return ServiceResponse<int>.Failure($"Payment exceeds remaining balance. Remaining balance is {remainingBalance:N2}.");
                }

                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "feesFiles");
                var fileUploadResult = FileUploadHandler.ProcessFile(record.evidenncefile, uploadDirectory);

                if (!fileUploadResult.Success)
                    return ServiceResponse<int>.Failure(fileUploadResult.Message);

                var payment = new OtherPayment
                {
                    Amount = record.amount,
                    ItemAmount = (decimal)feeSetup.Amount,
                    PayFeesSetUpId = feeSetup.Id,
                    TermRegId = record.termregid,
                    FilePath = $"feesFiles/{fileUploadResult.FileName}",
                    InvoiceNumber = $"OTH-{SD.GenerateUniqueNumber()}",
                    StaffUserId = GetCurrentUserId(),
                    Narration = record.narration ?? string.Empty,
                    Message = string.Empty
                };

                var previousPayments = _context.OtherPayments.Include(p => p.Termregistration).Where(k => k.TermRegId == record.termregid && k.PaymentState != GetEnums.PaymentState.Cancelled && k.Status != GetEnums.PaymentStatus.Rejected).ToList();
                if (previousPayments.Any())
                {
                    var totalamount = previousPayments.Sum(k => k.Amount);
                    var finalamount = totalamount + record.amount;

                    if (finalamount == (decimal)feeSetup.Amount)
                    {
                        payment.PaymentState = GetEnums.PaymentState.Completed;
                    }
                    else
                        payment.PaymentState = GetEnums.PaymentState.PartPayment;
                }

                if (record.amount == (decimal)feeSetup.Amount)
                    payment.PaymentState = GetEnums.PaymentState.Completed;
                else
                    payment.PaymentState = GetEnums.PaymentState.PartPayment;

                _context.OtherPayments.Add(payment);
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    return ServiceResponse<int>.Failure("An error occurred while recording the other payment. Please try again.");

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Other Payment",
                    entityType: "OtherPayment",
                    entityId: payment.Id.ToString(),
                    message: "Other payment recorded successfully and is pending approval.",
                    ipAddress: GetClientIpAddress(),
                    details: $"Student Reg Number: {record.regnumber}, Session ID: {record.session}, Term: {record.term}"
                );

                return ServiceResponse<int>.Success(payment.Id, "Other payment recorded successfully and is pending approval.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating other payment");
                return ServiceResponse<int>.Failure("An unexpected error occurred while processing the other payment.");
            }
        }

        public async Task<(List<FeesPaymentsDto> data, int recordsTotal, int recordsFiltered)> GetOtherPaymentsAsync(int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc", int? termFilter = null, int? sessionFilter = null, int? classFilter = null, int? subclassFilter = null)
        {
            try
            {
                var query = _context.OtherPayments
                    .Include(tr => tr.Termregistration.Student.ApplicationUser)
                    .Include(tr => tr.Termregistration.SchoolClass)
                    .Include(tr => tr.Termregistration.SessionYear)
                    .Include(tr => tr.OtherPayFeesSetUp.OtherPayItems)
                    .AsNoTracking()
                    .OrderByDescending(k => k.CreatedDate)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.Termregistration.Student.Surname.Contains(searchTerm) ||
                        x.Termregistration.Student.Firstname.Contains(searchTerm) ||
                        x.Termregistration.SchoolClass.Name.Contains(searchTerm) ||
                        x.Termregistration.SessionYear.Name.Contains(searchTerm));
                }

                if (termFilter.HasValue && termFilter > 0)
                {
                    query = query.Where(x => (int)x.Termregistration.Term == termFilter.Value);
                }

                if (sessionFilter.HasValue && sessionFilter > 0)
                {
                    query = query.Where(x => x.Termregistration.SessionId == sessionFilter.Value);
                }

                if (classFilter.HasValue && classFilter > 0)
                {
                    query = query.Where(x => x.Termregistration.SchoolClassId == classFilter.Value);
                }

                if (subclassFilter.HasValue && subclassFilter > 0)
                {
                    query = query.Where(x => x.Termregistration.SchoolSubclassId == subclassFilter.Value);
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
                        id = x.Id,
                        name = $"{x.Termregistration.Student.FullName}",
                        term = x.Termregistration.Term.ToString(),
                        session = x.Termregistration.SessionYear.Name,
                        schoolclass = x.Termregistration.SchoolClass.Name,
                        createdate = x.Termregistration.CreatedDate,
                        regnumber = x.Termregistration.Student.ApplicationUser.UserName,
                        fees = SD.ToNaira((decimal)x.OtherPayFeesSetUp.Amount),
                        amount = SD.ToNaira(x.Amount),
                        balance = SD.ToNaira(((decimal)x.OtherPayFeesSetUp.Amount - x.Amount)),
                        state = SD.GetSpanBadgeState(x.PaymentState),
                        status = SD.GetSpanBadgeStatus(x.Status),
                        paymentitem = x.OtherPayFeesSetUp.OtherPayItems.Name,
                        paymentitemid = x.OtherPayFeesSetUp.OtherPayId
                    })
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting other payments: {ex.Message}");
                return (new List<FeesPaymentsDto>(), 0, 0);
            }
        }

        public async Task<OtherPayment> GetOtherPaymentByIdAsync(int paymentId)
        {
            try
            {
                var payment = await _context.OtherPayments
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.OtherPayFeesSetUp.OtherPayItems)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting other payment by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<decimal> GetTotalPreviousOtherPaymentsAsync(int termRegId, int excludePaymentId = 0)
        {
            try
            {
                var total = await _context.OtherPayments
                    .Where(x => x.TermRegId == termRegId && x.Id != excludePaymentId && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != GetEnums.PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting total previous other payments: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<OtherPayment>> GetApprovedPaymentsByItemAsync(int termRegistrationId, int paymentSetUpId)
        {
            try
            {
                var payments = await _context.OtherPayments
                    .Where(x => x.TermRegId == termRegistrationId && 
                                x.PayFeesSetUpId == paymentSetUpId && 
                                x.Status == GetEnums.PaymentStatus.Approved)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                return payments ?? new List<OtherPayment>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting approved payments by item: {ex.Message}");
                return new List<OtherPayment>();
            }
        }

        public async Task<decimal> GetPreviousBalanceAsync(int termRegId)
        {
            try
            {
                var termRegistration = await _context.TermRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == termRegId);

                if (termRegistration == null)
                    return 0;

                var feeSetup = await _context.OtherPayFeesSetUp
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Term == termRegistration.Term && x.SchoolClassId == termRegistration.SchoolClassId && x.SessionId == termRegistration.SessionId);

                if (feeSetup == null)
                    return 0;

                var totalPaid = await _context.OtherPayments
                    .Where(x => x.TermRegId == termRegId && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != GetEnums.PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                return (decimal)feeSetup.Amount - totalPaid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting previous balance for other payments: {ex.Message}");
                return 0;
            }
        }

        public async Task<ServiceResponse<bool>> ApproveOtherPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.OtherPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != GetEnums.PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be approved.");

                payment.Status = GetEnums.PaymentStatus.Approved;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.OtherPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Approval",
                        entityType: "OtherPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Other Payment {payment.InvoiceNumber} has been approved.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Other payment approved successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to approve other payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving other payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while approving the other payment.");
            }
        }

        public async Task<ServiceResponse<bool>> RejectOtherPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.OtherPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != GetEnums.PaymentStatus.Pending)
                    return ServiceResponse<bool>.Failure("Only pending payments can be rejected.");

                payment.Status = GetEnums.PaymentStatus.Rejected;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.OtherPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Rejection",
                        entityType: "OtherPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Other Payment {payment.InvoiceNumber} has been rejected and removed.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Other payment rejected successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to reject other payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting other payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while rejecting the other payment.");
            }
        }

        public async Task<ServiceResponse<bool>> CancelOtherPaymentAsync(int paymentId)
        {
            try
            {
                var payment = await _context.OtherPayments.FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return ServiceResponse<bool>.Failure("Payment not found.");

                if (payment.Status != GetEnums.PaymentStatus.Rejected)
                    return ServiceResponse<bool>.Failure("Payment must be rejected before canceling.");

                if (payment.PaymentState == GetEnums.PaymentState.Cancelled)
                    return ServiceResponse<bool>.Failure("Payment is already cancelled.");

                payment.PaymentState = GetEnums.PaymentState.Cancelled;
                payment.UpdatedDate = DateTime.UtcNow;

                _context.OtherPayments.Update(payment);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Other Payment Cancellation",
                        entityType: "OtherPayment",
                        entityId: payment.Id.ToString(),
                        message: $"Other Payment {payment.InvoiceNumber} has been cancelled.",
                        ipAddress: GetClientIpAddress(),
                        details: $"Payment ID: {payment.Id}, Amount: {payment.Amount}"
                    );

                    return ServiceResponse<bool>.Success(true, "Other payment cancelled successfully.");
                }

                return ServiceResponse<bool>.Failure("Failed to cancel other payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error canceling other payment: {ex.Message}");
                return ServiceResponse<bool>.Failure("An error occurred while canceling the other payment.");
            }
        }

        #region OTHER ITEMS FEES SET UP
        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetOtherFeesSetUpAsync(int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.OtherPayFeesSetUp
                    .Include(x => x.Schoolclasses)
                    .Include(x => x.SessionYear)
                    .Include(x => x.OtherPayItems)
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(x =>
                        x.Schoolclasses.Name.Contains(searchValue) ||
                        x.SessionYear.Name.Contains(searchValue) ||
                        x.OtherPayItems.Name.Contains(searchValue) ||
                        x.Term.ToString().Contains(searchValue) ||
                        x.Amount.ToString().Contains(searchValue));
                }

                var recordsFiltered = await query.CountAsync();

                // Apply sorting
                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(x => x.Schoolclasses.Name) : query.OrderByDescending(x => x.Schoolclasses.Name),
                    2 => sortDirection == "asc" ? query.OrderBy(x => x.SessionYear.Name) : query.OrderByDescending(x => x.SessionYear.Name),
                    3 => sortDirection == "asc" ? query.OrderBy(x => x.Term) : query.OrderByDescending(x => x.Term),
                    4 => sortDirection == "asc" ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var data = await query
                    .Skip(start)
                    .Take(length)
                    .Select(x => new
                    {
                        id = x.Id,
                        term = x.Term.ToString(),
                        sessionname = x.SessionYear.Name,
                        classname = x.Schoolclasses.Name,
                        itemname = x.OtherPayItems.Name,
                        amount = SD.ToNaira((decimal)x.Amount),
                        createdate = x.CreatedDate
                    })
                    .Cast<dynamic>()
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting other fees setup list");
                return (new List<dynamic>(), 0, 0);
            }
        }

        public async Task<OtherPayFeesSetUp> GetOtherFeesSetUpByIdAsync(int paymentId)
        {
            var feeSetup = await _context.OtherPayFeesSetUp.AsNoTracking().FirstOrDefaultAsync(x => x.Id==paymentId);
            return feeSetup;
        }

        public async Task<(bool Succeeded, string Message)> DeleteOtherFeeSetUpAsync(int id)
        {
            try
            {
                if (id < 1)
                {
                    return (false, "Invalid data");
                }

                var hasPayments = await _context.OtherPayments
                    .AnyAsync(x => x.PayFeesSetUpId == id);

                if (hasPayments)
                {
                    return (false, "Cannot delete this fee. Payments exist under it.");
                }

                var feeSetup = await _context.OtherPayFeesSetUp
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (feeSetup == null)
                {
                    return (false, "Unknown fee setup information");
                }

                _context.OtherPayFeesSetUp.Remove(feeSetup);
                await _context.SaveChangesAsync();

                return (true, "Fee setup deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Other Fees set up delete");
                return (false, "An error occurred while deleting the fee setup");
            }
        }

        public async Task<PaymentDetailViewModel> GetOtherPaymentDetailAsync(int paymentId)
        {
            try
            {
                var payment = await _context.OtherPayments
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.Student)
                            .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.SchoolClass)
                    .Include(x => x.Termregistration)
                        .ThenInclude(x => x.SessionYear)
                    .Include(x => x.OtherPayFeesSetUp)
                        .ThenInclude(x => x.OtherPayItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                    return null;

                var previousPayments = await _context.OtherPayments
                    .Where(x => x.TermRegId == payment.TermRegId && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != GetEnums.PaymentStatus.Rejected && x.Id != paymentId)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var totalPaidIncludingCurrent = previousPayments + payment.Amount;
                var newBalance = (decimal)payment.OtherPayFeesSetUp.Amount - totalPaidIncludingCurrent;

                var viewModel = new PaymentDetailViewModel
                {
                    Id = payment.Id,
                    PaymentId = payment.Id,
                    InvoiceNumber = payment.InvoiceNumber,
                    PaymentReference = string.Empty,
                    CreatedDate = payment.CreatedDate,
                    UpdatedDate = payment.UpdatedDate,

                    // Student Information
                    StudentName = payment.Termregistration?.Student?.FullName ?? string.Empty,
                    StudentRegNumber = payment.Termregistration?.Student?.ApplicationUser?.UserName ?? string.Empty,
                    StudentClass = payment.Termregistration?.SchoolClass?.Name ?? string.Empty,
                    ClassName = payment.Termregistration?.SchoolClass?.Name ?? string.Empty,

                    // Academic Information
                    Term = payment.Termregistration?.Term.ToString() ?? string.Empty,
                    Session = payment.Termregistration?.SessionYear?.Name ?? string.Empty,
                    SessionName = payment.Termregistration?.SessionYear?.Name ?? string.Empty,

                    // Amount Details
                    TotalFees = (decimal)payment.OtherPayFeesSetUp.Amount,
                    TotalPaidBefore = (decimal)previousPayments,
                    PreviousPayments = (decimal)previousPayments,
                    PreviousBalance = ((decimal)payment.OtherPayFeesSetUp.Amount) - (decimal)previousPayments,
                    CurrentPayment = payment.Amount,
                    AmountPaid = payment.Amount,
                    Amount = payment.Amount,
                    NewBalance = newBalance > 0 ? newBalance : 0,
                    ItemAmount = (decimal)payment.OtherPayFeesSetUp.Amount,

                    // Payment Item
                    PaymentItemName = payment.OtherPayFeesSetUp?.OtherPayItems?.Name ?? string.Empty,

                    // Payment Method
                    PaymentChannel = "Direct",

                    // Status Information
                    ApprovalStatus = payment.Status,
                    PaymentStatus = payment.Status.ToString(),
                    Status = payment.Status.ToString(),
                    PaymentState = payment.PaymentState,
                    PaymentStateString = payment.PaymentState.ToString(),

                    // Evidence/File
                    FilePath = payment.FilePath ?? string.Empty,
                    EvidenceFileName = Path.GetFileName(payment.FilePath) ?? string.Empty,
                    EvidenceFilePath = payment.FilePath ?? string.Empty,

                    // Additional Information
                    Message = payment.Message ?? string.Empty,
                    Notes = payment.Narration ?? string.Empty,
                    Narration = payment.Narration ?? string.Empty,

                    // Staff/Approval Info
                    StaffUserId = payment.StaffUserId ?? string.Empty,
                    StaffName = string.Empty,
                    StaffEmail = string.Empty,
                    CanApprove = payment.Status == GetEnums.PaymentStatus.Pending,
                    CanReject = payment.Status == GetEnums.PaymentStatus.Pending,
                    CanCancel = payment.Status == GetEnums.PaymentStatus.Rejected,

                    // Date for display
                    PaymentDate = payment.CreatedDate
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting other payment detail: {ex.Message}");
                return null;
            }
        }

        public async Task<ServiceResponse<RecordPaymentViewModel>> SearchOtherPaymentAsync(RecordPaymeentSearchViewModel searchModel)
        {
            try
            {
                if (searchModel == null)
                    return ServiceResponse<RecordPaymentViewModel>.Failure("Invalid search criteria.");

                // Query TermRegistrations based on search criteria
                var query = _context.TermRegistrations
                    .Include(x => x.Student)
                        .ThenInclude(x => x.ApplicationUser)
                    .Include(x => x.SchoolClass)
                    .Include(x => x.SessionYear)
                    .AsNoTracking()
                    .AsQueryable();

                // Apply filter by Term if provided
                if (searchModel.term > 0)
                {
                    if (Enum.TryParse<GetEnums.Term>(searchModel.term.ToString(), out var termEnum))
                    {
                        query = query.Where(x => x.Term == termEnum);
                    }
                }

                // Apply filter by Session if provided
                if (searchModel.sessionid > 0)
                {
                    query = query.Where(x => x.SessionId == searchModel.sessionid);
                }

                // Apply filter by Class if provided
                if (searchModel.schoolclass > 0)
                {
                    query = query.Where(x => x.SchoolClassId == searchModel.schoolclass);
                }

                // Apply filter by Registration Number if provided
                if (!string.IsNullOrWhiteSpace(searchModel.StudentRegNumber))
                {
                    query = query.Where(x => x.Student.ApplicationUser.UserName.Contains(searchModel.StudentRegNumber));
                }

                // Get the matching registration
                var registration = await query.FirstOrDefaultAsync();

                if (registration == null)
                    return ServiceResponse<RecordPaymentViewModel>.Failure("No student found with the provided search criteria.");

                // Get the fee setup for this registration
                var feeSetup = await _context.OtherPayFeesSetUp
                    .Include(x => x.OtherPayItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => 
                        x.Term == registration.Term && 
                        x.SchoolClassId == registration.SchoolClassId && 
                        x.SessionId == registration.SessionId);

                if (feeSetup == null)
                    return ServiceResponse<RecordPaymentViewModel>.Failure("Other payment fees have not been configured for the selected class and session.");

                // Calculate total paid before
                var totalPaidBefore = await _context.OtherPayments
                    .Where(x => x.TermRegId == registration.Id && x.PaymentState != GetEnums.PaymentState.Cancelled && x.Status != GetEnums.PaymentStatus.Rejected)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                var balance = (decimal)feeSetup.Amount - totalPaidBefore;

                // Map to RecordPaymentViewModel
                var result = new RecordPaymentViewModel
                {
                    termregid = registration.Id,
                    FeesSetUpid = feeSetup.Id,
                    name = registration.Student.FullName,
                    regnumber = registration.Student.ApplicationUser.UserName,
                    session = registration.SessionYear.Name,
                    term = registration.Term.ToString(),
                    schoolclass = registration.SchoolClass.Name,
                    amount = 0,
                    feespayment = (decimal)feeSetup.Amount,
                    balance = balance,
                    narration = string.Empty,
                    otherPayment = feeSetup.OtherPayItems.Name,
                    totalamount = totalPaidBefore
                };

                return ServiceResponse<RecordPaymentViewModel>.Success(result, "Student found successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching other payment: {ex.Message}");
                return ServiceResponse<RecordPaymentViewModel>.Failure("An error occurred while searching for the student.");
            }
        }
        #endregion
    }
}
