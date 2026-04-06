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
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class FinanceServices : IFinanceServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FinanceServices> _logger;

        public FinanceServices(ApplicationDbContext context, ILogService logService, IHttpContextAccessor httpContextAccessor, ILogger<FinanceServices> logger)
        {
            _context = context;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetFeesSetupAsync(int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.TermlyFeesSetups
                    .Include(x => x.SessionYear)
                    .Include(x => x.SchoolClass)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.SchoolClass.Name.Contains(searchTerm) ||
                        x.SessionYear.Name.Contains(searchTerm) ||
                        x.Term.ToString().Contains(searchTerm) ||
                        x.Amount.ToString().Contains(searchTerm)
                    );
                }

                int recordsTotal = await _context.TermlyFeesSetups.CountAsync();
                int recordsFiltered = await query.CountAsync();

                // Apply sorting
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id);

                // Apply pagination
                var data = await query.Skip(skip).Take(pageSize).Select(x => new FeeSetupDto
                {
                    id = x.Id,
                    classname = x.SchoolClass.Name,
                    sessionname = x.SessionYear.Name,
                    term1 = x.Term.ToString(),
                    amount = x.Amount,
                    createdate = x.CreatedDate,
                    Term = x.Term,
                    sessionid = x.SessionId,
                    amount1 = SD.ToNaira(x.Amount),
                    classid = x.SchoolClassId
                })
                    .ToListAsync();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving fees setup data: {ex.Message}", ex);
            }
        }

        public async Task<FeesSetupViewModel> GetFeesSetupByIdAsync(int id)
        {
            try
            {
                var feesSetup = await _context.TermlyFeesSetups
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (feesSetup == null)
                    return null;

                return new FeesSetupViewModel
                {
                    Id = feesSetup.Id,
                    Amount = feesSetup.Amount,
                    Term = feesSetup.Term,
                    SchoolClassId = feesSetup.SchoolClassId,
                    SessionId = feesSetup.SessionId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving fees setup: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<int>> CreateFeesSetupAsync(FeesSetupViewModel model)
        {
            try
            {
                // Check if record already exists
                var existingRecord = await _context.TermlyFeesSetups
                    .FirstOrDefaultAsync(x => x.SchoolClassId == model.SchoolClassId && x.SessionId == model.SessionId && x.Term == model.Term);

                if (existingRecord != null)
                {
                    // Log duplicate attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Create",
                        entityType: "FeesSetup",
                        entityId: "0",
                        message: "Attempted to create duplicate fees setup (rejected)",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}, Term: {model.Term}"
                    );

                    return ServiceResponse<int>.Failure("Fees setup already exists for this class, session, and term");
                }

                var feesSetup = new TermlyFeesSetup
                {
                    Amount = model.Amount,
                    Term = model.Term,
                    SchoolClassId = model.SchoolClassId,
                    SessionId = model.SessionId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.TermlyFeesSetups.Add(feesSetup);
                await _context.SaveChangesAsync();

                // Log successful creation
                var className = await _context.SchoolClasses
                    .Where(x => x.Id == model.SchoolClassId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var sessionName = await _context.SessionYears
                    .Where(x => x.Id == model.SessionId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Create",
                    entityType: "FeesSetup",
                    entityId: feesSetup.Id.ToString(),
                    message: $"Fees setup created: {className} | {sessionName} | Term {model.Term} | Amount: ?{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"ID: {feesSetup.Id}, Class: {className}, Session: {sessionName}, Term: {model.Term}, Amount: {model.Amount}"
                );

                return ServiceResponse<int>.Success(feesSetup.Id, "Fees setup created successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "Fees Setup Creation Error",
                    message: "Error creating fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<int>.Failure($"Error creating fees setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UpdateFeesSetupAsync(FeesSetupViewModel model)
        {
            try
            {
                var feesSetup = await _context.TermlyFeesSetups
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (feesSetup == null)
                {
                    // Log not found attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Update",
                        entityType: "FeesSetup",
                        entityId: model.Id.ToString(),
                        message: "Update attempt failed - Fees setup not found",
                        ipAddress: GetClientIpAddress(),
                        details: $"Attempted to update non-existent fees setup ID: {model.Id}"
                    );

                    return ServiceResponse<bool>.Failure("Fees setup not found");
                }

                // Check if another record with same details exists
                var existingRecord = await _context.TermlyFeesSetups.FirstOrDefaultAsync(x => x.Id != model.Id && x.SchoolClassId == model.SchoolClassId && x.SessionId == model.SessionId && x.Term == model.Term);

                if (existingRecord != null)
                {
                    // Log duplicate attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Update",
                        entityType: "FeesSetup",
                        entityId: model.Id.ToString(),
                        message: "Update attempt failed - Duplicate fees setup exists",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}, Term: {model.Term}"
                    );

                    return ServiceResponse<bool>.Failure("Another fees setup already exists with these details");
                }

                // Store old values for audit trail
                var oldAmount = feesSetup.Amount;
                var oldTerm = feesSetup.Term;
                var oldClassId = feesSetup.SchoolClassId;
                var oldSessionId = feesSetup.SessionId;

                // Update values
                feesSetup.Amount = model.Amount;
                feesSetup.Term = model.Term;
                feesSetup.SchoolClassId = model.SchoolClassId;
                feesSetup.SessionId = model.SessionId;
                feesSetup.UpdatedDate = DateTime.UtcNow;

                _context.TermlyFeesSetups.Update(feesSetup);
                await _context.SaveChangesAsync();

                // Log successful update
                var className = await _context.SchoolClasses
                    .Where(x => x.Id == model.SchoolClassId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var sessionName = await _context.SessionYears
                    .Where(x => x.Id == model.SessionId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var changeDetails = new System.Text.StringBuilder();
                if (oldAmount != model.Amount)
                    changeDetails.AppendLine($"Amount: {oldAmount} ? {model.Amount}");
                if (oldTerm != model.Term)
                    changeDetails.AppendLine($"Term: {oldTerm} ? {model.Term}");
                if (oldClassId != model.SchoolClassId)
                    changeDetails.AppendLine($"Class ID: {oldClassId} ? {model.SchoolClassId}");
                if (oldSessionId != model.SessionId)
                    changeDetails.AppendLine($"Session ID: {oldSessionId} ? {model.SessionId}");

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Update",
                    entityType: "FeesSetup",
                    entityId: model.Id.ToString(),
                    message: $"Fees setup updated: {className} | {sessionName} | New Amount: ?{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"ID: {model.Id}, Class: {className}, Session: {sessionName}\n{changeDetails}"
                );

                return ServiceResponse<bool>.Success(true, "Fees setup updated successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "Fees Setup Update Error",
                    message: "Error updating fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure($"Error updating fees setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteFeesSetupAsync(int id)
        {
            try
            {
                // Check for related fee payment records
                var hasPayments = await _context.FeesPayments.AnyAsync(x => x.TermlyFeesId == id);
                if (hasPayments)
                {
                    // Log deletion attempt blocked by related records
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "FeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - Related fee payment records exist",
                        ipAddress: GetClientIpAddress(),
                        details: $"Cannot delete fees setup ID {id} - Has existing fee payment records"
                    );

                    return ServiceResponse<bool>.Failure("This term's fee setup already has existing fee records. Please review and try again.");
                }

                var feesSetup = await _context.TermlyFeesSetups
                    .Include(x => x.SchoolClass)
                    .Include(x => x.SessionYear)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (feesSetup == null)
                {
                    // Log not found attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "FeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - Fees setup not found",
                        ipAddress: GetClientIpAddress(),
                        details: $"Attempted to delete non-existent fees setup ID: {id}"
                    );

                    return ServiceResponse<bool>.Failure("Fees setup not found");
                }

                // Store details for audit trail before deletion
                var className = feesSetup.SchoolClass?.Name ?? "Unknown";
                var sessionName = feesSetup.SessionYear?.Name ?? "Unknown";
                var amount = feesSetup.Amount;
                var term = feesSetup.Term;

                _context.TermlyFeesSetups.Remove(feesSetup);
                await _context.SaveChangesAsync();

                // Log successful deletion
                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Delete",
                    entityType: "FeesSetup",
                    entityId: id.ToString(),
                    message: $"Fees setup deleted: {className} | {sessionName} | Term {term} | Amount: ₦{SD.ToNaira(amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"Deleted fees setup ID: {id}\nClass: {className}, Session: {sessionName}, Term: {term}, Amount: {amount}"
                );

                return ServiceResponse<bool>.Success(true, "Fees setup deleted successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "Fees Setup Delete Error",
                    message: "Error deleting fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure($"Error deleting fees setup: {ex.Message}");
            }
        }

        public async Task<ViewSelections> GetFeesSetupSelectionsAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var sessions = await _context.SessionYears
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var terms = Enum.GetValues(typeof(Term))
                    .Cast<Term>()
                    .Select(x => new SelectListItem
                    {
                        Value = ((int)x).ToString(),
                        Text = x.ToString()
                    })
                    .ToList();
                var subclass = await _context.SchoolSubClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                return new ViewSelections
                {
                    SchoolClasses = schoolClasses,
                    AcademicSession = sessions,
                    Terms = terms,
                    SubClass = subclass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving selections: {ex.Message}", ex);
            }
        }

        #region PTA Fees Setup Methods

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPTAFeesSetupAsync(int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.PTAFeesSetups
                    .Include(x => x.SessionYear)
                    .Include(x => x.SchoolClass)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.SchoolClass.Name.Contains(searchTerm) ||
                        x.SessionYear.Name.Contains(searchTerm) ||
                        x.Term.ToString().Contains(searchTerm) ||
                        x.Amount.ToString().Contains(searchTerm)
                    );
                }

                int recordsTotal = await _context.PTAFeesSetups.CountAsync();
                int recordsFiltered = await query.CountAsync();

                // Apply sorting
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id);

                // Apply pagination
                var data = await query.Skip(skip).Take(pageSize).Select(x => new
                {
                    id = x.Id,
                    classname = x.SchoolClass.Name,
                    sessionname = x.SessionYear.Name,
                    term1 = x.Term.ToString(),
                    amount = x.Amount,
                    createdate = x.CreatedDate,
                    Term = x.Term,
                    sessionid = x.SessionId,
                    amount1 = SD.ToNaira(x.Amount),
                    classid = x.SchoolClassId
                })
                    .ToListAsync();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving PTA fees setup data: {ex.Message}", ex);
            }
        }

        public async Task<PTAFeesSetupViewModel> GetPTAFeesSetupByIdAsync(int id)
        {
            try
            {
                var ptaFeesSetup = await _context.PTAFeesSetups
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (ptaFeesSetup == null)
                    return null;

                return new PTAFeesSetupViewModel
                {
                    Id = ptaFeesSetup.Id,
                    Amount = ptaFeesSetup.Amount,
                    Term = (int)ptaFeesSetup.Term,
                    SchoolClassId = ptaFeesSetup.SchoolClassId,
                    SessionId = ptaFeesSetup.SessionId
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving PTA fees setup: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<int>> CreatePTAFeesSetupAsync(PTAFeesSetupViewModel model)
        {
            try
            {
                // Check if record already exists
                var existingRecord = await _context.PTAFeesSetups
                    .FirstOrDefaultAsync(x => x.SchoolClassId == model.SchoolClassId && x.SessionId == model.SessionId && (int)x.Term == model.Term);

                if (existingRecord != null)
                {
                    // Log duplicate attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Create",
                        entityType: "PTAFeesSetup",
                        entityId: "0",
                        message: "Attempted to create duplicate PTA fees setup (rejected)",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}, Term: {model.Term}"
                    );

                    return ServiceResponse<int>.Failure("PTA fees setup already exists for this class, session, and term");
                }

                var ptaFeesSetup = new PTAFeesSetup
                {
                    Amount = model.Amount,
                    Term = (Term)model.Term,
                    SchoolClassId = model.SchoolClassId,
                    SessionId = model.SessionId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.PTAFeesSetups.Add(ptaFeesSetup);
                await _context.SaveChangesAsync();

                // Log successful creation
                var className = await _context.SchoolClasses
                    .Where(x => x.Id == model.SchoolClassId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var sessionName = await _context.SessionYears
                    .Where(x => x.Id == model.SessionId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Create",
                    entityType: "PTAFeesSetup",
                    entityId: ptaFeesSetup.Id.ToString(),
                    message: $"PTA fees setup created: {className} | {sessionName} | Term {(Term)model.Term} | Amount: ₦{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"ID: {ptaFeesSetup.Id}, Class: {className}, Session: {sessionName}, Term: {model.Term}, Amount: {model.Amount}"
                );

                return ServiceResponse<int>.Success(ptaFeesSetup.Id, "PTA fees setup created successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "PTA Fees Setup Creation Error",
                    message: "Error creating PTA fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<int>.Failure($"Error creating PTA fees setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UpdatePTAFeesSetupAsync(PTAFeesSetupViewModel model)
        {
            try
            {
                var ptaFeesSetup = await _context.PTAFeesSetups
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (ptaFeesSetup == null)
                {
                    // Log not found attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Update",
                        entityType: "PTAFeesSetup",
                        entityId: model.Id.ToString(),
                        message: "Update attempt failed - PTA fees setup not found",
                        ipAddress: GetClientIpAddress(),
                        details: $"Attempted to update non-existent PTA fees setup ID: {model.Id}"
                    );

                    return ServiceResponse<bool>.Failure("PTA fees setup not found");
                }

                // Check if another record with same details exists
                var existingRecord = await _context.PTAFeesSetups.FirstOrDefaultAsync(x => x.Id != model.Id && x.SchoolClassId == model.SchoolClassId && x.SessionId == model.SessionId && (int)x.Term == model.Term);

                if (existingRecord != null)
                {
                    // Log duplicate attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Update",
                        entityType: "PTAFeesSetup",
                        entityId: model.Id.ToString(),
                        message: "Update attempt failed - Duplicate PTA fees setup exists",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}, Term: {model.Term}"
                    );

                    return ServiceResponse<bool>.Failure("Another PTA fees setup already exists with these details");
                }

                // Store old values for audit trail
                var oldAmount = ptaFeesSetup.Amount;
                var oldTerm = ptaFeesSetup.Term;
                var oldClassId = ptaFeesSetup.SchoolClassId;
                var oldSessionId = ptaFeesSetup.SessionId;

                // Update values
                ptaFeesSetup.Amount = model.Amount;
                ptaFeesSetup.Term = (Term)model.Term;
                ptaFeesSetup.SchoolClassId = model.SchoolClassId;
                ptaFeesSetup.SessionId = model.SessionId;
                ptaFeesSetup.UpdatedDate = DateTime.UtcNow;

                _context.PTAFeesSetups.Update(ptaFeesSetup);
                await _context.SaveChangesAsync();

                // Log successful update
                var className = await _context.SchoolClasses
                    .Where(x => x.Id == model.SchoolClassId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var sessionName = await _context.SessionYears
                    .Where(x => x.Id == model.SessionId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var changeDetails = new System.Text.StringBuilder();
                if (oldAmount != model.Amount)
                    changeDetails.AppendLine($"Amount: {oldAmount} → {model.Amount}");
                if (oldTerm != (Term)model.Term)
                    changeDetails.AppendLine($"Term: {oldTerm} → {model.Term}");
                if (oldClassId != model.SchoolClassId)
                    changeDetails.AppendLine($"Class ID: {oldClassId} → {model.SchoolClassId}");
                if (oldSessionId != model.SessionId)
                    changeDetails.AppendLine($"Session ID: {oldSessionId} → {model.SessionId}");

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Update",
                    entityType: "PTAFeesSetup",
                    entityId: model.Id.ToString(),
                    message: $"PTA fees setup updated: {className} | {sessionName} | New Amount: ₦{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"ID: {model.Id}, Class: {className}, Session: {sessionName}\n{changeDetails}"
                );

                return ServiceResponse<bool>.Success(true, "PTA fees setup updated successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "PTA Fees Setup Update Error",
                    message: "Error updating PTA fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure($"Error updating PTA fees setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeletePTAFeesSetupAsync(int id)
        {
            try
            {
                // Check for related PTA payment records
                var hasPayments = await _context.PTAFeesPayments.AnyAsync(x => x.PtaFeesSetupId == id);
                if (hasPayments)
                {
                    // Log deletion attempt blocked by related records
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "PTAFeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - Related PTA payment records exist",
                        ipAddress: GetClientIpAddress(),
                        details: $"Cannot delete PTA fees setup ID {id} - Has existing PTA payment records"
                    );

                    return ServiceResponse<bool>.Failure("This PTA fees setup already has existing payment records. Please review and try again.");
                }

                var ptaFeesSetup = await _context.PTAFeesSetups
                    .Include(x => x.SchoolClass)
                    .Include(x => x.SessionYear)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (ptaFeesSetup == null)
                {
                    // Log not found attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "PTAFeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - PTA fees setup not found",
                        ipAddress: GetClientIpAddress(),
                        details: $"Attempted to delete non-existent PTA fees setup ID: {id}"
                    );

                    return ServiceResponse<bool>.Failure("PTA fees setup not found");
                }

                // Store details for audit trail before deletion
                var className = ptaFeesSetup.SchoolClass?.Name ?? "Unknown";
                var sessionName = ptaFeesSetup.SessionYear?.Name ?? "Unknown";
                var amount = ptaFeesSetup.Amount;
                var term = ptaFeesSetup.Term;

                _context.PTAFeesSetups.Remove(ptaFeesSetup);
                await _context.SaveChangesAsync();

                // Log successful deletion
                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Delete",
                    entityType: "PTAFeesSetup",
                    entityId: id.ToString(),
                    message: $"PTA fees setup deleted: {className} | {sessionName} | Term {term} | Amount: ₦{SD.ToNaira(amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"Deleted PTA fees setup ID: {id}\nClass: {className}, Session: {sessionName}, Term: {term}, Amount: {amount}"
                );

                return ServiceResponse<bool>.Success(true, "PTA fees setup deleted successfully");
            }
            catch (Exception ex)
            {
                // Log error
                await _logService.LogErrorAsync(
                    subject: "PTA Fees Setup Delete Error",
                    message: "Error deleting PTA fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure($"Error deleting PTA fees setup: {ex.Message}");
            }
        }

        public async Task<ViewSelections> GetPTAFeesSetupSelectionsAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var sessions = await _context.SessionYears
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var terms = Enum.GetValues(typeof(Term))
                    .Cast<Term>()
                    .Select(x => new SelectListItem
                    {
                        Value = ((int)x).ToString(),
                        Text = x.ToString()
                    })
                    .ToList();

                var subclass = await _context.SchoolSubClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                return new ViewSelections
                {
                    SchoolClasses = schoolClasses,
                    AcademicSession = sessions,
                    Terms = terms,
                    SubClass = subclass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving PTA selections: {ex.Message}", ex);
            }
        }

        public async Task<ViewSelections> GetPTAPaymentSelectionsAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var sessions = await _context.SessionYears
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var terms = Enum.GetValues(typeof(Term))
                    .Cast<Term>()
                    .Select(x => new SelectListItem
                    {
                        Value = ((int)x).ToString(),
                        Text = x.ToString()
                    })
                    .ToList();

                var subclass = await _context.SchoolSubClasses
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                return new ViewSelections
                {
                    SchoolClasses = schoolClasses,
                    AcademicSession = sessions,
                    Terms = terms,
                    SubClass = subclass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving PTA payment selections: {ex.Message}", ex);
            }
        }

        #endregion

        #region Other Fees Setup Methods

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetOtherFeesSetupAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc")
        {
            try
            {
                var query = _context.OtherPayFeesSetUp
                    .AsNoTracking()
                    .Include(x => x.Schoolclasses)
                    .Include(x => x.SessionYear)
                    .Include(x => x.OtherPayItems)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.Schoolclasses.Name.Contains(searchTerm) ||
                        x.SessionYear.Name.Contains(searchTerm) ||
                        x.OtherPayItems.Name.Contains(searchTerm));
                }

                var recordsTotal = await _context.OtherPayFeesSetUp.CountAsync();
                var recordsFiltered = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
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
                _logger.LogError(ex, "Error retrieving other fees setup list");
                throw;
            }
        }

        public async Task<OtherFeesSetupViewModel> GetOtherFeesSetupByIdAsync(int id)
        {
            try
            {
                var otherFeesSetup = await _context.OtherPayFeesSetUp
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (otherFeesSetup == null)
                    return null;

                return new OtherFeesSetupViewModel
                {
                    Id = otherFeesSetup.Id,
                    SchoolClassId = otherFeesSetup.SchoolClassId,
                    SessionId = otherFeesSetup.SessionId,
                    Term = (int)otherFeesSetup.Term,
                    OtherPayItemId = otherFeesSetup.OtherPayId,
                    Amount = (decimal)otherFeesSetup.Amount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving other fees setup by ID");
                throw;
            }
        }

        public async Task<ServiceResponse<int>> CreateOtherFeesSetupAsync(OtherFeesSetupViewModel model)
        {
            try
            {
                // Check if record already exists for the same class, session, term, and payment item
                var existingRecord = await _context.OtherPayFeesSetUp
                    .FirstOrDefaultAsync(x => x.SchoolClassId == model.SchoolClassId 
                        && x.SessionId == model.SessionId 
                        && (int)x.Term == model.Term
                        && x.OtherPayId == model.OtherPayItemId);

                if (existingRecord != null)
                {
                    // Log duplicate attempt
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Create",
                        entityType: "OtherFeesSetup",
                        entityId: "0",
                        message: "Attempted to create duplicate other fees setup (rejected)",
                        ipAddress: GetClientIpAddress(),
                        details: $"Class ID: {model.SchoolClassId}, Session ID: {model.SessionId}, Term: {model.Term}, Payment Item ID: {model.OtherPayItemId}"
                    );

                    return ServiceResponse<int>.Failure("Other fees setup already exists for this class, session, term, and payment item");
                }

                var otherFeesSetup = new OtherPayFeesSetUp
                {
                    SchoolClassId = model.SchoolClassId,
                    SessionId = model.SessionId,
                    Term = (Term)model.Term,
                    OtherPayId = model.OtherPayItemId,
                    Amount = (double)model.Amount,
                    CreatedDate = DateTime.UtcNow
                };

                _context.OtherPayFeesSetUp.Add(otherFeesSetup);
                await _context.SaveChangesAsync();

                // Get related data for logging
                var className = await _context.SchoolClasses
                    .Where(x => x.Id == model.SchoolClassId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var sessionName = await _context.SessionYears
                    .Where(x => x.Id == model.SessionId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                var itemName = await _context.OtherPayItemsTable
                    .Where(x => x.Id == model.OtherPayItemId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Unknown";

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Create",
                    entityType: "OtherFeesSetup",
                    entityId: otherFeesSetup.Id.ToString(),
                    message: $"Other fees setup created: {className} | {sessionName} | Term {(Term)model.Term} | Item: {itemName} | Amount: ₦{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"ID: {otherFeesSetup.Id}, Class: {className}, Session: {sessionName}, Item: {itemName}, Term: {model.Term}, Amount: {model.Amount}"
                );

                return ServiceResponse<int>.Success(otherFeesSetup.Id, "Other fees setup created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating other fees setup");
                await _logService.LogErrorAsync(
                    subject: "Other Fees Setup Creation Error",
                    message: "Error creating other fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<int>.Failure("An error occurred while creating the other fees setup");
            }
        }

        public async Task<ServiceResponse<bool>> UpdateOtherFeesSetupAsync(OtherFeesSetupViewModel model)
        {
            try
            {
                var otherFeesSetup = await _context.OtherPayFeesSetUp.FirstOrDefaultAsync(x => x.Id == model.Id);

                if (otherFeesSetup == null)
                    return ServiceResponse<bool>.Failure("Other fees setup not found");

                otherFeesSetup.SchoolClassId = model.SchoolClassId;
                otherFeesSetup.SessionId = model.SessionId;
                otherFeesSetup.Term = (Term)model.Term;
                otherFeesSetup.OtherPayId = model.OtherPayItemId;
                otherFeesSetup.Amount = (double)model.Amount;
                otherFeesSetup.CreatedDate = DateTime.Now;

                _context.OtherPayFeesSetUp.Update(otherFeesSetup);
                await _context.SaveChangesAsync();

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Update",
                    entityType: "OtherFeesSetup",
                    entityId: otherFeesSetup.Id.ToString(),
                    message: $"Other fees setup updated - Amount: ₦{SD.ToNaira(model.Amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"Updated other fees setup ID: {otherFeesSetup.Id}"
                );

                return ServiceResponse<bool>.Success(true, "Other fees setup updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating other fees setup");
                await _logService.LogErrorAsync(
                    subject: "Other Fees Setup Update Error",
                    message: "Error updating other fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure("An error occurred while updating the other fees setup");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteOtherFeesSetupAsync(int id)
        {
            try
            {
                var existingPayments = await _context.OtherPayments
                    .AsNoTracking()
                    .Where(x => x.PayFeesSetUpId == id)
                    .AnyAsync();

                if (existingPayments)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "OtherFeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - Related other payment records exist",
                        ipAddress: GetClientIpAddress(),
                        details: $"Cannot delete other fees setup ID {id} - Has existing other payment records"
                    );

                    return ServiceResponse<bool>.Failure("This other fees setup already has existing payment records. Please review and try again.");
                }

                var otherFeesSetup = await _context.OtherPayFeesSetUp
                    .Include(x => x.Schoolclasses)
                    .Include(x => x.SessionYear)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (otherFeesSetup == null)
                {
                    await _logService.LogUserActionAsync(
                        userId: GetCurrentUserId(),
                        userName: GetCurrentUserName(),
                        action: "Delete",
                        entityType: "OtherFeesSetup",
                        entityId: id.ToString(),
                        message: "Delete attempt failed - Other fees setup not found",
                        ipAddress: GetClientIpAddress(),
                        details: $"Attempted to delete non-existent other fees setup ID: {id}"
                    );

                    return ServiceResponse<bool>.Failure("Other fees setup not found");
                }

                var className = otherFeesSetup.Schoolclasses?.Name ?? "Unknown";
                var sessionName = otherFeesSetup.SessionYear?.Name ?? "Unknown";
                var amount = otherFeesSetup.Amount;
                var term = otherFeesSetup.Term;

                _context.OtherPayFeesSetUp.Remove(otherFeesSetup);
                await _context.SaveChangesAsync();

                await _logService.LogUserActionAsync(
                    userId: GetCurrentUserId(),
                    userName: GetCurrentUserName(),
                    action: "Delete",
                    entityType: "OtherFeesSetup",
                    entityId: id.ToString(),
                    message: $"Other fees setup deleted: {className} | {sessionName} | Term {term} | Amount: ₦{SD.ToNaira((decimal)amount)}",
                    ipAddress: GetClientIpAddress(),
                    details: $"Deleted other fees setup ID: {id}\nClass: {className}, Session: {sessionName}, Term: {term}, Amount: {amount}"
                );

                return ServiceResponse<bool>.Success(true, "Other fees setup deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting other fees setup");
                await _logService.LogErrorAsync(
                    subject: "Other Fees Setup Delete Error",
                    message: "Error deleting other fees setup",
                    details: ex.Message,
                    userId: GetCurrentUserId(),
                    ipAddress: GetClientIpAddress()
                );

                return ServiceResponse<bool>.Failure("An error occurred while deleting the other fees setup");
            }
        }

        public async Task<ViewSelections> GetOtherFeesSetupSelectionsAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var sessions = await _context.SessionYears
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var terms = Enum.GetValues(typeof(Term))
                    .Cast<Term>()
                    .Select(x => new SelectListItem
                    {
                        Value = ((int)x).ToString(),
                        Text = x.ToString()
                    })
                    .ToList();

                var paymentItems = await _context.OtherPayItemsTable
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var subclass = await _context.SchoolSubClasses
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                return new ViewSelections
                {
                    SchoolClasses = schoolClasses,
                    AcademicSession = sessions,
                    Terms = terms,
                    PaymentItems = paymentItems,
                    SubClass = subclass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving other fees setup selections: {ex.Message}", ex);
            }
        }

        public async Task<ViewSelections> GetOtherPaymentSelectionsAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var sessions = await _context.SessionYears
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                var terms = Enum.GetValues(typeof(Term))
                    .Cast<Term>()
                    .Select(x => new SelectListItem
                    {
                        Value = ((int)x).ToString(),
                        Text = x.ToString()
                    })
                    .ToList();

                var subclass = await _context.SchoolSubClasses
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToListAsync();

                return new ViewSelections
                {
                    SchoolClasses = schoolClasses,
                    AcademicSession = sessions,
                    Terms = terms,
                    SubClass = subclass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving other payment selections: {ex.Message}", ex);
            }
        }
    }
}

        #endregion

