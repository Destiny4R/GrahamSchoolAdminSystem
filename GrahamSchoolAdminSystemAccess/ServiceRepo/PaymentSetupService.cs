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
    public class PaymentSetupService : IPaymentSetupService
    {
        private readonly ApplicationDbContext _context;

        public PaymentSetupService(ApplicationDbContext context)
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

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetPaymentSetupsAsync(
            int skip = 0, int pageSize = 10, string searchTerm = "", int sortColumn = 0, string sortDirection = "asc",
            int? sessionFilter = null, int? termFilter = null, int? classFilter = null)
        {
            try
            {
                var query = _context.PaymentSetups
                    .Include(x => x.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(x => x.Session)
                    .Include(x => x.SchoolClass)
                    .AsQueryable();

                if (sessionFilter.HasValue && sessionFilter.Value > 0)
                    query = query.Where(x => x.SessionId == sessionFilter.Value);
                if (termFilter.HasValue && termFilter.Value > 0)
                    query = query.Where(x => x.Term == (Term)termFilter.Value);
                if (classFilter.HasValue && classFilter.Value > 0)
                    query = query.Where(x => x.ClassId == classFilter.Value);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.PaymentItem.Name.Contains(searchTerm) ||
                        x.PaymentItem.PaymentCategory.Name.Contains(searchTerm) ||
                        x.Session.Name.Contains(searchTerm) ||
                        x.SchoolClass.Name.Contains(searchTerm));
                }

                int recordsTotal = await _context.PaymentSetups.CountAsync();
                int recordsFiltered = await query.CountAsync();

                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id);

                var rawData = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        PaymentItemName = x.PaymentItem.Name,
                        CategoryName = x.PaymentItem.PaymentCategory.Name,
                        SessionName = x.Session.Name,
                        x.Term,
                        ClassName = x.SchoolClass.Name,
                        x.Amount,
                        x.IsActive,
                        CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                var data = rawData.Select(x => new
                {
                    x.Id,
                    x.PaymentItemName,
                    x.CategoryName,
                    x.SessionName,
                    TermName = GetTermName(x.Term),
                    x.ClassName,
                    Amount = SD.ToNaira(x.Amount),
                    x.IsActive,
                    x.CreatedAt
                }).ToList();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment setups: {ex.Message}", ex);
            }
        }

        public async Task<PaymentSetupViewModel> GetPaymentSetupByIdAsync(int id)
        {
            try
            {
                var setup = await _context.PaymentSetups
                    .Include(x => x.PaymentItem).ThenInclude(pi => pi.PaymentCategory)
                    .Include(x => x.Session)
                    .Include(x => x.SchoolClass)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null) return null;

                return new PaymentSetupViewModel
                {
                    Id = setup.Id,
                    PaymentItemId = setup.PaymentItemId,
                    SessionId = setup.SessionId,
                    Term = setup.Term,
                    ClassId = setup.ClassId,
                    Amount = setup.Amount,
                    IsActive = setup.IsActive,
                    CategoryId = setup.PaymentItem?.CategoryId,
                    PaymentItemName = setup.PaymentItem?.Name,
                    SessionName = setup.Session?.Name,
                    TermName = GetTermName(setup.Term),
                    ClassName = setup.SchoolClass?.Name,
                    CategoryName = setup.PaymentItem?.PaymentCategory?.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment setup: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<int>> CreatePaymentSetupAsync(PaymentSetupViewModel model)
        {
            try
            {
                if (model.Amount <= 0)
                    return ServiceResponse<int>.Failure("Amount must be greater than 0");

                var itemExists = await _context.PaymentItems.AnyAsync(x => x.Id == model.PaymentItemId && x.IsActive);
                if (!itemExists)
                    return ServiceResponse<int>.Failure("Selected payment item does not exist or is inactive");

                // Check uniqueness constraint
                var duplicate = await _context.PaymentSetups.AnyAsync(x =>
                    x.PaymentItemId == model.PaymentItemId &&
                    x.SessionId == model.SessionId &&
                    x.Term == model.Term &&
                    x.ClassId == model.ClassId);

                if (duplicate)
                    return ServiceResponse<int>.Failure("A payment setup already exists for this combination of item, session, term, and class");

                var setup = new PaymentSetup
                {
                    PaymentItemId = model.PaymentItemId,
                    SessionId = model.SessionId,
                    Term = model.Term,
                    ClassId = model.ClassId,
                    Amount = model.Amount,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PaymentSetups.Add(setup);
                await _context.SaveChangesAsync();

                return ServiceResponse<int>.Success(setup.Id, "Payment setup created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.Failure($"Error creating payment setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UpdatePaymentSetupAsync(PaymentSetupViewModel model)
        {
            try
            {
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (setup == null)
                    return ServiceResponse<bool>.Failure("Payment setup not found");

                if (model.Amount <= 0)
                    return ServiceResponse<bool>.Failure("Amount must be greater than 0");

                // Check uniqueness (excluding current record)
                var duplicate = await _context.PaymentSetups.AnyAsync(x =>
                    x.PaymentItemId == model.PaymentItemId &&
                    x.SessionId == model.SessionId &&
                    x.Term == model.Term &&
                    x.ClassId == model.ClassId &&
                    x.Id != model.Id);

                if (duplicate)
                    return ServiceResponse<bool>.Failure("A payment setup already exists for this combination of item, session, term, and class");

                setup.PaymentItemId = model.PaymentItemId;
                setup.SessionId = model.SessionId;
                setup.Term = model.Term;
                setup.ClassId = model.ClassId;
                setup.Amount = model.Amount;
                setup.IsActive = model.IsActive;
                setup.UpdatedAt = DateTime.UtcNow;

                _context.PaymentSetups.Update(setup);
                await _context.SaveChangesAsync();

                return ServiceResponse<bool>.Success(true, "Payment setup updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error updating payment setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeletePaymentSetupAsync(int id)
        {
            try
            {
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null)
                    return ServiceResponse<bool>.Failure("Payment setup not found");

                _context.PaymentSetups.Remove(setup);
                await _context.SaveChangesAsync();

                return ServiceResponse<bool>.Success(true, "Payment setup deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error deleting payment setup: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> TogglePaymentSetupAsync(int id)
        {
            try
            {
                var setup = await _context.PaymentSetups.FirstOrDefaultAsync(x => x.Id == id);
                if (setup == null)
                    return ServiceResponse<bool>.Failure("Payment setup not found");

                setup.IsActive = !setup.IsActive;
                setup.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var status = setup.IsActive ? "activated" : "deactivated";
                return ServiceResponse<bool>.Success(true, $"Payment setup {status} successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error toggling payment setup: {ex.Message}");
            }
        }
    }
}
