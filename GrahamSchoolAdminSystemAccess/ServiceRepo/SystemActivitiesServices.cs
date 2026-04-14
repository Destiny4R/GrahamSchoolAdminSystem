using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class SystemActivitiesServices : ISystemActivitiesServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SystemActivitiesServices> _logger;

        public SystemActivitiesServices(ApplicationDbContext context, ILogger<SystemActivitiesServices> logger)
        {
            this._context = context;
            this._logger = logger;
        }
        // School Sub Class implementations
        public async Task<(List<SchoolSubClassDto> data, int recordsTotal, int recordsFiltered)> GetSchoolSubClassesAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection)
        {
            var query = _context.SchoolSubClasses.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(e => e.Name.Contains(searchValue));
            }

            var recordsFiltered = await query.CountAsync();

            query = query.OrderByDescending(e => e.CreatedDate);

            var data = await query.Skip(start).Take(length)
                .Select(s => new SchoolSubClassDto { id = s.Id, name = s.Name, createdate = s.CreatedDate })
                .ToListAsync();

            return (data, recordsTotal, recordsFiltered);
        }

        public async Task<(bool Succeeded, string Message)> CreateSchoolSubClassAsync(SchoolSubClassViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();

                if (await _context.SchoolSubClasses.AnyAsync(x => x.Name == name))
                    return (false, "Sub class already exists");

                var entity = new SchoolSubClass { Name = name, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow };
                _context.SchoolSubClasses.Add(entity);
                await _context.SaveChangesAsync();

                return (true, "Sub class created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("CREATE SUBCLASS ERROR", ex);
                return (false, "An unexpected error occurred while creating the sub class");
            }
        }

        public async Task<(bool Succeeded, string Message)> UpdateSchoolSubClassAsync(SchoolSubClassViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();
                var entity = await _context.SchoolSubClasses.FirstOrDefaultAsync(k => k.Id == model.Id);

                if (entity == null) 
                    return (false, "Sub class not found");

                if (await _context.SchoolSubClasses.AnyAsync(x => x.Name == name && x.Id != model.Id))
                    return (false, "Another sub class with same name exists");

                entity.Name = name;
                entity.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, "Sub class updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("UPDATE SUBCLASS ERROR", ex);
                return (false, "An unexpected error occurred while updating the sub class");
            }
        }

        public async Task<(bool Succeeded, string Message)> DeleteSchoolSunClassAsync(int id, string message)
        {
            try
            {
                if (id <= 0) 
                    return (false, "Invalid sub class selection");

                var entity = await _context.SchoolSubClasses.FirstOrDefaultAsync(k => k.Id == id);

                if (entity == null) 
                    return (false, "Unknown sub class, check and try again");

                _context.SchoolSubClasses.Remove(entity);
                var result = await _context.SaveChangesAsync();

                if (result <= 0) 
                    return (false, "An error occurred while deleting the sub class");

                return (true, "Sub class successfully removed");
            }
            catch (Exception ex)
            {
                _logger.LogError("DELETE SUBCLASS ERROR", ex);
                return (false, "An unexpected error occurred while deleting the sub class");
            }
        }

        // Academic session implementations
        public async Task<(List<SessionYearDto> data, int recordsTotal, int recordsFiltered)> GetAcademicSessionAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection)
        {
            var query = _context.SessionYears.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(e => e.Name.Contains(searchValue));
            }

            var recordsFiltered = await query.CountAsync();

            query = query.OrderByDescending(e => e.CreatedDate);

            var data = await query.Skip(start).Take(length)
                .Select(s => new SessionYearDto { id = s.Id, name = s.Name, createdate = s.CreatedDate })
                .ToListAsync();

            return (data, recordsTotal, recordsFiltered);
        }

        public async Task<(bool Succeeded, string Message)> CreateAcademicSessionAsync(SessionYearViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();
                if (await _context.SessionYears.AnyAsync(x => x.Name == name))
                    return (false, "Academic session already exists");

                var entity = new SessionYear { Name = name, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow };
                _context.SessionYears.Add(entity);
                await _context.SaveChangesAsync();

                return (true, "Academic session created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("CREATE ACADEMIC SESSION ERROR", ex);
                return (false, "An unexpected error occurred while creating the academic session");
            }
        }

        public async Task<(bool Succeeded, string Message)> UpdateAcademicSessionAsync(SessionYearViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();
                var entity = await _context.SessionYears.FirstOrDefaultAsync(k => k.Id == model.Id);
                if (entity == null) return (false, "Academic session not found");
                if (await _context.SessionYears.AnyAsync(x => x.Name == name && x.Id != model.Id))
                    return (false, "Another academic session with same name exists");

                entity.Name = name;
                entity.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, "Academic session updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("UPDATE ACADEMIC SESSION ERROR", ex);
                return (false, "An unexpected error occurred while updating the academic session");
            }
        }

        public async Task<(bool Succeeded, string Message)> DeleteAcademicSessionAsync(int id, string message)
        {
            try
            {
                if (id <= 0) 
                    return (false, "Invalid academic session selection");

                if (await _context.TermRegistrations.AnyAsync(x => x.SessionId == id))
                    return (false, "Students are registered in this session. Remove them before proceeding");

                var entity = await _context.SessionYears.FirstOrDefaultAsync(k => k.Id == id);
                if (entity == null) 
                    return (false, "Unknown academic session, check and try again");

                _context.SessionYears.Remove(entity);
                var result = await _context.SaveChangesAsync();

                if (result <= 0) 
                    return (false, "An error occurred while deleting the academic session");

                return (true, "Academic session successfully removed");
            }
            catch (Exception ex)
            {
                _logger.LogError("DELETE ACADEMIC SESSION ERROR", ex);
                return (false, "An unexpected error occurred while deleting the academic session");
            }
        }

        //SCHOOL CLASS IMPLEMENTATIONS
        public async Task<(bool Succeeded, string Message)> CreateSchoolClassAsync(SchoolClassViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();

                if (await _context.SchoolClasses.AnyAsync(x => x.Name == name))
                {
                    return (false, "Class already exists");
                }

                var entity = new SchoolClasses
                {
                    Name = name,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.SchoolClasses.Add(entity);
                await _context.SaveChangesAsync();

                return (true, "Class created successfully");
            }
            catch (Exception ex)
            {
                // TODO: log ex
                _logger.LogError("CLASS CREATING ERROR", ex);
                return (false, "An unexpected error occurred while creating the class");
            }
        }

        public async Task<(bool Succeeded, string Message)> DeleteSchoolClassAsync(int id, string message)
        {
            try
            {
                if (id <= 0)
                    return (false, "Invalid class selection");

                if (await _context.TermRegistrations.AnyAsync(x => x.SchoolClassId == id))
                {
                    return (false, "Students are registered in this class. Remove them before proceeding");
                }

                var entity = await _context.SchoolClasses.FirstOrDefaultAsync(k => k.Id == id);

                if (entity == null)
                    return (false, "Unknown class, check and try again");

                _context.SchoolClasses.Remove(entity);

                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    return (false, "An error occurred while deleting the class");

                return (true, "Class successfully removed");
            }
            catch (Exception ex)
            {
                // TODO: log ex
                _logger.LogError("DELETE CLASS ERROR", ex);
                return (false, "An unexpected error occurred while deleting the class");
            }
        }

        public async Task<(List<SchoolClassesDto> data, int recordsTotal, int recordsFiltered)> GetSchoolClassesAsync(int start, int length, string searchValue, int sortColumnIndex, string sortDirection)
        {
            var query = _context.SchoolClasses.AsNoTracking().AsQueryable();

            var recordsTotal = await query.CountAsync();

            // Filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(e => e.Name.Contains(searchValue));
            }

            var recordsFiltered = await query.CountAsync();

            // Sorting
            if (sortColumnIndex > 0)
            {
                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                    2 => sortDirection == "asc" ? query.OrderBy(e => e.CreatedDate) : query.OrderByDescending(e => e.CreatedDate),
                    _ => query.OrderByDescending(e => e.CreatedDate) // Default to newest first
                };
            }
            else
            {
                // Default ordering by newest
                query = query.OrderByDescending(e => e.CreatedDate);
            }

            // Paging and projection
            var dataViewModel = await query.Skip(start).Take(length)
                .Select(room => new SchoolClassesDto
                {
                    id = room.Id,
                    name = room.Name,
                    createdate = room.CreatedDate                    
                }).ToListAsync();


            return (dataViewModel, recordsTotal, recordsFiltered);
        }

        public async Task<(bool Succeeded, string Message)> UpdateSchoolClassAsync(SchoolClassViewModel model)
        {
            try
            {
                var name = model.Name?.Trim();

                var entity = await _context.SchoolClasses.FirstOrDefaultAsync(k => k.Id == model.Id);

                if (entity == null)
                    return (false, "Class not found");

                if (await _context.SchoolClasses.AnyAsync(x => x.Name == name && x.Id != model.Id))
                {
                    return (false, "Another class with same name exists");
                }

                entity.Name = name;
                entity.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "Class updated successfully"); // ✅ FIXED
            }
            catch (Exception ex)
            {
                // TODO: log ex
                _logger.LogError("CLASS UPDATE ERROR", ex);
                return (false, "An unexpected error occurred while updating the class");
            }
        }



    }
}
