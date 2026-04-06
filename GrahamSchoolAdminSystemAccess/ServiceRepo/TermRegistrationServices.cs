using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class TermRegistrationServices : ITermRegistrationServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TermRegistrationServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public TermRegistrationServices(
            ApplicationDbContext context,
            ILogger<TermRegistrationServices> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<StudentViewModel> GetStudentByUsernameAsync(string username)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(s => s.ApplicationUser.UserName == username);

                if (student == null)
                    return null;

                return new StudentViewModel
                {
                    Id = student.Id,
                    Surname = student.Surname,
                    Firstname = student.Firstname,
                    Othername = student.Othername,
                    GenderId = (int)student.Gender,
                    ApplicationUserId = student.ApplicationUserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student by username: {ex.Message}");
                return null;
            }
        }

        public async Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> GetStudentTermRegistrationAsync(
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
                var query = _context.TermRegistrations
                    .Include(tr => tr.Student.ApplicationUser)
                    .Include(tr => tr.SchoolClass)
                    .Include(tr => tr.SessionYear)
                    .AsNoTracking()
                    .OrderByDescending(k=>k.CreatedDate)
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x =>
                        x.Student.Surname.Contains(searchTerm) ||
                        x.Student.Firstname.Contains(searchTerm) ||
                        x.SchoolClass.Name.Contains(searchTerm) ||
                        x.SessionYear.Name.Contains(searchTerm));
                }

                // Apply filters if provided
                if (termFilter.HasValue && termFilter > 0)
                {
                    query = query.Where(x => (int)x.Term == termFilter.Value);
                }

                if (sessionFilter.HasValue && sessionFilter > 0)
                {
                    query = query.Where(x => x.SessionId == sessionFilter.Value);
                }

                if (classFilter.HasValue && classFilter > 0)
                {
                    query = query.Where(x => x.SchoolClassId == classFilter.Value);
                }

                if (subclassFilter.HasValue && subclassFilter > 0)
                {
                    query = query.Where(x => x.SchoolSubclassId == subclassFilter.Value);
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
                    .Select(x => new TermRegDto
                    {
                        id = x.Id,
                        name = $"{x.Student.Firstname} {x.Student.Surname}",
                        term = x.Term.ToString(),
                        session = x.SessionYear.Name,
                        schoolclass = x.SchoolClass.Name,
                        createdate = x.CreatedDate,
                        regnumber = x.Student.ApplicationUser.UserName
                    })
                    .ToListAsync();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting term registrations: {ex.Message}");
                return (new List<TermRegDto>(), 0, 0);
            }
        }

        public async Task<TermRegistrationViewModel> GetStudentTermRegistrationByIdAsync(int id)
        {
            try
            {
                var termReg = await _context.TermRegistrations
                    .Include(tr => tr.Student.ApplicationUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (termReg == null)
                    return null;

                return new TermRegistrationViewModel
                {
                    Id = termReg.Id,
                    SchoolClassId = termReg.SchoolClassId,
                    SessionId = termReg.SessionId,
                    Term = termReg.Term,
                    SchoolSubclassId = termReg.SchoolSubclassId,
                    StudentId = termReg.StudentId,
                    StudentName = $"{termReg.Student.Firstname} {termReg.Student.Surname}",
                    StudentRegNumber = termReg.Student.ApplicationUser.UserName.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting term registration by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TermRegistrationViewModel>> GetAllStudentTermRegistrationsAsync()
        {
            try
            {
                var termRegs = await _context.TermRegistrations
                    .Include(tr => tr.Student)
                    .AsNoTracking()
                    .ToListAsync();

                return termRegs.Select(x => new TermRegistrationViewModel
                {
                    Id = x.Id,
                    SchoolClassId = x.SchoolClassId,
                    SessionId = x.SessionId,
                    Term = x.Term,
                    SchoolSubclassId = x.SchoolSubclassId,
                    StudentId = x.StudentId,
                    StudentName = $"{x.Student.Firstname} {x.Student.Surname}",
                    StudentRegNumber = x.Student.Id.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all term registrations: {ex.Message}");
                return new List<TermRegistrationViewModel>();
            }
        }

        public async Task<ServiceResponse<int>> CreateStudentTermRegistrationAsync(TermRegistrationViewModel model)
        {
            try
            {
                // Validate student exists
                var student = await _context.Students.FirstOrDefaultAsync(k=>k.Id==model.StudentId);
                if (student == null)
                    return ServiceResponse<int>.Failure("Student not found");

                // Validate school class exists
                var schoolClass = await _context.SchoolClasses.FindAsync(model.SchoolClassId);
                if (schoolClass == null)
                    return ServiceResponse<int>.Failure("School class not found");

                // Validate session exists
                var session = await _context.SessionYears.FindAsync(model.SessionId);
                if (session == null)
                    return ServiceResponse<int>.Failure("Academic session not found");

                // Validate school subclass exists
                var subClass = await _context.SchoolSubClasses.FindAsync(model.SchoolSubclassId);
                if (subClass == null)
                    return ServiceResponse<int>.Failure("School subclass not found");


                // Business Logic: Check if student already has this subclass in this session
                var existingSubClassReg = await _context.TermRegistrations
                    .AnyAsync(x => x.StudentId == model.StudentId && 
                                   x.SessionId == model.SessionId &&
                                   x.SchoolSubclassId == model.SchoolSubclassId &&
                                   x.SchoolSubclassId == model.SchoolSubclassId &&
                                   x.Term == model.Term);

                if (existingSubClassReg)
                    return ServiceResponse<int>.Failure("Student is already registered for this class in this academic session and term.");


                var termRegistration = new TermRegistration
                {
                    StudentId = model.StudentId,
                    SchoolClassId = model.SchoolClassId,
                    SessionId = model.SessionId,
                    Term = model.Term,
                    SchoolSubclassId = model.SchoolSubclassId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.TermRegistrations.Add(termRegistration);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Term registration created successfully for student ID: {model.StudentId}");
                return ServiceResponse<int>.Success(termRegistration.Id, "Term registration created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating term registration: {ex.Message}");
                return ServiceResponse<int>.Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UpdateStudentTermRegistrationAsync(TermRegistrationViewModel model)
        {
            try
            {
                var termReg = await _context.TermRegistrations.FindAsync(model.Id);
                if (termReg == null)
                    return ServiceResponse<bool>.Failure("Term registration not found");

                // Validate school class exists
                var schoolClass = await _context.SchoolClasses.FindAsync(model.SchoolClassId);
                if (schoolClass == null)
                    return ServiceResponse<bool>.Failure("School class not found");

                // Validate subclass exists
                var subClass = await _context.SchoolSubClasses.FindAsync(model.SchoolSubclassId);
                if (subClass == null)
                    return ServiceResponse<bool>.Failure("School subclass not found");

                // Business Logic: If changing class, check if student already has another class in this session
                if (termReg.SchoolClassId != model.SchoolClassId)
                {
                    var conflictingClassReg = await _context.TermRegistrations
                        .AnyAsync(x => x.StudentId == model.StudentId &&
                                       x.SessionId == model.SessionId &&
                                       x.Id != model.Id &&
                                       x.SchoolClassId != model.SchoolClassId);

                    if (conflictingClassReg)
                        return ServiceResponse<bool>.Failure("Cannot change class. Student is already registered for another class in this session.");
                }

                termReg.SchoolClassId = model.SchoolClassId;
                termReg.SchoolSubclassId = model.SchoolSubclassId;
                termReg.Term = model.Term;
                termReg.UpdatedDate = DateTime.UtcNow;

                _context.TermRegistrations.Update(termReg);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Term registration updated successfully for ID: {model.Id}");
                return ServiceResponse<bool>.Success(true, "Term registration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating term registration: {ex.Message}");
                return ServiceResponse<bool>.Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteStudentTermRegistrationAsync(int id)
        {
            try
            {
                var termReg = await _context.TermRegistrations.FindAsync(id);
                if (termReg == null)
                    return ServiceResponse<bool>.Failure("Term registration not found");

                _context.TermRegistrations.Remove(termReg);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Term registration deleted successfully for ID: {id}");
                return ServiceResponse<bool>.Success(true, "Term registration deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting term registration: {ex.Message}");
                return ServiceResponse<bool>.Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteStudentsTermRegistrationAsync(List<int> ids)
        {
            try
            {
                var termRegs = await _context.TermRegistrations
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                if (!termRegs.Any())
                    return ServiceResponse<bool>.Failure("No term registrations found to delete");

                _context.TermRegistrations.RemoveRange(termRegs);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bulk deleted {termRegs.Count} term registrations");
                return ServiceResponse<bool>.Success(true, $"Successfully deleted {termRegs.Count} term registration(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error bulk deleting term registrations: {ex.Message}");
                return ServiceResponse<bool>.Failure($"An error occurred: {ex.Message}");
            }
        }

        public async Task<List<TermRegistration>> GetOldTermRegRecordsAsync(SelectOptionsData Selectsearch)
        {
            var data = _context.TermRegistrations.Include(r=>r.Student.ApplicationUser).Include(n => n.SchoolClass).Include(n => n.SchoolSubClass).Include(n=>n.SessionYear).Where(k=> (int)k.Term==Selectsearch.term && k.SessionId==Selectsearch.sessionid && k.SchoolClassId==Selectsearch.schoolclass && k.SchoolSubclassId==Selectsearch.subclass);
            return await data.ToListAsync();
        }
    }
}
