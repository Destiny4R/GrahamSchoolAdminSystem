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

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class SchoolClassServices : ISchoolClassServices
    {
        private readonly ApplicationDbContext _context;

        public SchoolClassServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetSchoolClassesAsync(
            int skip = 0,
            int pageSize = 10,
            string searchTerm = "",
            int sortColumn = 0,
            string sortDirection = "asc")
        {
            try
            {
                var query = _context.SchoolClasses.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(x =>
                        x.Name.Contains(searchTerm)
                    );
                }

                int recordsTotal = await _context.SchoolClasses.CountAsync();
                int recordsFiltered = await query.CountAsync();

                // Apply sorting
                query = sortDirection.ToLower() == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id);

                // Apply pagination
                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.CreatedDate,
                        x.UpdatedDate,
                        CreatedDateFormatted = x.CreatedDate.ToString("dd/MM/yyyy hh:mm tt"),
                        UpdatedDateFormatted = x.UpdatedDate.ToString("dd/MM/yyyy hh:mm tt")
                    })
                    .ToListAsync();

                return (data.Cast<dynamic>().ToList(), recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving school classes: {ex.Message}", ex);
            }
        }

        public async Task<SchoolClassViewModel> GetSchoolClassByIdAsync(int id)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (schoolClass == null)
                    return null;

                return new SchoolClassViewModel
                {
                    Id = schoolClass.Id,
                    Name = schoolClass.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving school class: {ex.Message}", ex);
            }
        }

        public async Task<List<SchoolClassViewModel>> GetAllSchoolClassesAsync()
        {
            try
            {
                var schoolClasses = await _context.SchoolClasses
                    .OrderBy(x => x.Name)
                    .Select(x => new SchoolClassViewModel
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToListAsync();

                return schoolClasses;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all school classes: {ex.Message}", ex);
            }
        }

        public async Task<ServiceResponse<int>> CreateSchoolClassAsync(SchoolClassViewModel model)
        {
            try
            {
                // Check if class name already exists
                var exists = await ClassNameExistsAsync(model.Name);
                if (exists)
                    return ServiceResponse<int>.Failure("A class with this name already exists");

                var schoolClass = new SchoolClasses
                {
                    Name = model.Name.Trim(),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.SchoolClasses.Add(schoolClass);
                await _context.SaveChangesAsync();

                return ServiceResponse<int>.Success(schoolClass.Id, "School class created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<int>.Failure($"Error creating school class: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UpdateSchoolClassAsync(SchoolClassViewModel model)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (schoolClass == null)
                    return ServiceResponse<bool>.Failure("School class not found");

                // Check if another class with same name exists
                var exists = await ClassNameExistsAsync(model.Name, model.Id);
                if (exists)
                    return ServiceResponse<bool>.Failure("Another class with this name already exists");

                schoolClass.Name = model.Name.Trim();
                schoolClass.UpdatedDate = DateTime.UtcNow;

                _context.SchoolClasses.Update(schoolClass);
                await _context.SaveChangesAsync();

                return ServiceResponse<bool>.Success(true, "School class updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error updating school class: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteSchoolClassAsync(int id)
        {
            try
            {
                var schoolClass = await _context.SchoolClasses
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (schoolClass == null)
                    return ServiceResponse<bool>.Failure("School class not found");

                // Check if class has related data (students registered in this class)
                var hasStudents = await _context.TermRegistrations.AnyAsync(x => x.SchoolClassId == id);

                if (hasStudents)
                    return ServiceResponse<bool>.Failure("Cannot delete class with related records (student registrations)");

                _context.SchoolClasses.Remove(schoolClass);
                await _context.SaveChangesAsync();

                return ServiceResponse<bool>.Success(true, "School class deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Failure($"Error deleting school class: {ex.Message}");
            }
        }

        public async Task<bool> ClassNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.SchoolClasses.Where(x => x.Name.ToLower() == name.ToLower());

                if (excludeId.HasValue)
                    query = query.Where(x => x.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking class name: {ex.Message}", ex);
            }
        }
    }
}
