using ClosedXML.Excel;
using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class StudentServices : IStudentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentServices(ApplicationDbContext context, ILogger<StudentServices> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<(List<StudentViewModel> data, int recordsTotal, int recordsFiltered)> GetStudentsAsync(
            int start = 0, int length = 10, string searchValue = "", int sortColumnIndex = 0, string sortDirection = "asc")
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.ApplicationUser)
                    .AsNoTracking()
                    .AsQueryable();

                var recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(x =>
                        x.Surname.Contains(searchValue) ||
                        x.Firstname.Contains(searchValue) ||
                        x.Othername.Contains(searchValue) ||
                        x.ApplicationUser.Email.Contains(searchValue));
                }

                var recordsFiltered = await query.CountAsync();

                query = sortColumnIndex switch
                {
                    1 => sortDirection == "asc" ? query.OrderBy(x => x.Surname) : query.OrderByDescending(x => x.Surname),
                    2 => sortDirection == "asc" ? query.OrderBy(x => x.Firstname) : query.OrderByDescending(x => x.Firstname),
                    3 => sortDirection == "asc" ? query.OrderBy(x => x.CreatedDate) : query.OrderByDescending(x => x.CreatedDate),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var students = await query.Skip(start).Take(length).ToListAsync();
                var data = students.Select(MapToStudentViewModel).ToList();

                return (data, recordsTotal, recordsFiltered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students");
                return (new List<StudentViewModel>(), 0, 0);
            }
        }

        public async Task<StudentViewModel> GetStudentByIdAsync(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == studentId);

                if (student == null)
                    return null;

                return MapToStudentViewModel(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by id {StudentId}", studentId);
                return null;
            }
        }

        public async Task<(bool Succeeded, string Message, object Data)> CreateStudentAsync(StudentViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return (false, "A user account with this Student Reg. Number already exists", null);

                // Create the ApplicationUser (login account)
                var applicationUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(applicationUser, "12345678");
                if (!identityResult.Succeeded)
                {
                    var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to create user account for {Email}: {Errors}", model.Email, errors);

                    await transaction.RollbackAsync();
                    return (false, $"Failed to create user account: {errors}", null);
                }

                // Create student record
                var student = new StudentTable
                {
                    Surname = model.Surname?.Trim(),
                    Firstname = model.Firstname?.Trim(),
                    Othername = model.Othername?.Trim() ?? string.Empty,
                    Gender = (GetEnums.Gender)(model.GenderId ?? 0),
                    PaspportPath = model.PassportPath ?? string.Empty,
                    ApplicationUserId = applicationUser.Id,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return (true, "User and student created successfully", student);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error creating user and student for {Email}", model.Email);
                return (false, "An unexpected error occurred", null);
            }
        }

        public async Task<(bool Succeeded, string Message)> UpdateStudentAsync(StudentViewModel model)
        {
            try
            {
                var student = await _context.Students.Include(s => s.ApplicationUser).FirstOrDefaultAsync(x => x.Id == model.Id);

                if (student == null)
                    return (false, "Student not found");

                // Check if email is already used by another user
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null && existingUser.Id != student.ApplicationUserId)
                    return (false, "Student Reg. Number is already in use by another account");

                // Update student fields
                student.Surname = model.Surname?.Trim();
                student.Firstname = model.Firstname?.Trim();
                student.Othername = model.Othername?.Trim()?? string.Empty;

                if (model.GenderId.HasValue)
                    student.Gender = (GetEnums.Gender)model.GenderId.Value;

                // Update the user's email if changed
                if (student.ApplicationUser != null && student.ApplicationUser.Email != model.Email)
                {
                    student.ApplicationUser.Email = model.Email;
                    student.ApplicationUser.UserName = model.Email;
                    student.ApplicationUser.NormalizedEmail = model.Email.ToUpperInvariant();
                    student.ApplicationUser.NormalizedUserName = model.Email.ToUpperInvariant();
                }

                await _context.SaveChangesAsync();
                return (true, "Student updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId}", model.Id);
                return (false, "Error updating student");
            }
        }

        public async Task<(bool Succeeded, string Message, List<StudentImportResult> Results)> ImportStudentsFromExcelAsync(IFormFile excelFile)
        {
            var results = new List<StudentImportResult>();

            if (excelFile == null || excelFile.Length == 0)
                return (false, "No file uploaded", results);

            try
            {
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                // Get the range of data (skip header row)
                var firstRowUsed = worksheet.FirstRowUsed().RowNumber() + 1;
                var lastRowUsed = worksheet.LastRowUsed().RowNumber();

                for (int row = firstRowUsed; row <= lastRowUsed; row++)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        // Skip empty rows
                        if (worksheet.Cell(row, 1).IsEmpty())
                            continue;

                        var email = worksheet.Cell(row, 1).GetString().Trim();
                        var surname = worksheet.Cell(row, 2).GetString().Trim();
                        var firstname = worksheet.Cell(row, 3).GetString().Trim();
                        var othername = worksheet.Cell(row, 4).GetString().Trim();
                        var genderValue = worksheet.Cell(row, 5).GetString().Trim();
                        var passportPath = worksheet.Cell(row, 6).GetString().Trim();

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            results.Add(new StudentImportResult
                            {
                                RowNumber = row,
                                Succeeded = false,
                                Message = "Email is required",
                                Email = ""
                            });
                            await transaction.RollbackAsync();
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(firstname))
                        {
                            results.Add(new StudentImportResult
                            {
                                RowNumber = row,
                                Succeeded = false,
                                Message = "Surname and Firstname are required",
                                Email = email
                            });
                            await transaction.RollbackAsync();
                            continue;
                        }

                        var genderId = SD.ParseGenderId(genderValue);
                        if (!genderId.HasValue || genderId == 0)
                        {
                            results.Add(new StudentImportResult
                            {
                                RowNumber = row,
                                Succeeded = false,
                                Message = "Invalid gender value. Use 'Male' or 'Female'",
                                Email = email
                            });
                            await transaction.RollbackAsync();
                            continue;
                        }

                        // Check if user already exists
                        var existingUser = await _userManager.FindByEmailAsync(email);
                        if (existingUser != null)
                        {
                            results.Add(new StudentImportResult
                            {
                                RowNumber = row,
                                Succeeded = false,
                                Message = "A user account with this email already exists",
                                Email = email
                            });
                            await transaction.RollbackAsync();
                            continue;
                        }

                        // Create the ApplicationUser (login account)
                        var applicationUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true
                        };

                        var identityResult = await _userManager.CreateAsync(applicationUser, "12345678");

                        if (!identityResult.Succeeded)
                        {
                            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                            _logger.LogWarning("Failed to create user account for {Email}: {Errors}", email, errors);

                            results.Add(new StudentImportResult
                            {
                                RowNumber = row,
                                Succeeded = false,
                                Message = $"Failed to create user account: {errors}",
                                Email = email
                            });

                            await transaction.RollbackAsync();
                            continue;
                        }

                        // Create student record
                        var student = new StudentTable
                        {
                            Surname = surname,
                            Firstname = firstname,
                            Othername = othername ?? string.Empty,
                            Gender = (GetEnums.Gender)genderId.Value,
                            PaspportPath = passportPath ?? string.Empty,
                            ApplicationUserId = applicationUser.Id,
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();

                        results.Add(new StudentImportResult
                        {
                            RowNumber = row,
                            Succeeded = true,
                            Message = "Student created successfully",
                            Email = email
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        results.Add(new StudentImportResult
                        {
                            RowNumber = row,
                            Succeeded = false,
                            Message = $"Error processing row: {ex.Message}",
                            Email = worksheet.Cell(row, 1).GetString()
                        });

                        _logger.LogError(ex, "Error importing student at row {Row}", row);
                    }
                }

                var successCount = results.Count(r => r.Succeeded);
                var failureCount = results.Count(r => !r.Succeeded);

                return (true,
                        $"Import completed: {successCount} successful, {failureCount} failed",
                        results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing students from Excel");
                return (false, $"Error reading Excel file: {ex.Message}", results);
            }
        }




        public async Task<(bool Succeeded, string Message)> DeleteStudentAsync(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(x => x.Id == studentId);

                if (student == null)
                    return (false, "Student not found");

                var applicationUserId = student.ApplicationUserId;

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                // Delete the associated user account
                if (!string.IsNullOrWhiteSpace(applicationUserId))
                {
                    var user = await _userManager.FindByIdAsync(applicationUserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                            _logger.LogWarning("Student deleted but failed to remove user account {UserId}", applicationUserId);
                    }
                }

                return (true, "Student and user account deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", studentId);
                return (false, "Error deleting student");
            }
        }

        public async Task<(bool Succeeded, string Message)> ToggleStudentActivationAsync(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.Id == studentId);

                if (student == null)
                    return (false, "Student not found");

                if (student.ApplicationUser == null)
                    return (false, "Student user account not found");

                var user = student.ApplicationUser;

                // Determine current state
                var isLocked = user.LockoutEnabled &&
                               user.LockoutEnd.HasValue &&
                               user.LockoutEnd > DateTimeOffset.UtcNow;

                if (isLocked)
                {
                    // ✅ Activate (unlock)
                    user.LockoutEnd = null;
                    user.LockoutEnabled = false;
                    user.AccessFailedCount = 0;

                    await _context.SaveChangesAsync();
                    return (true, "Student account activated successfully");
                }
                else
                {
                    // ✅ Deactivate (lock indefinitely)
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    user.LockoutEnabled = true;

                    await _context.SaveChangesAsync();
                    return (true, "Student account deactivated successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling activation for student {StudentId}", studentId);
                return (false, "Error updating student account status");
            }
        }

        #region Helper Methods

        private StudentViewModel MapToStudentViewModel(StudentTable student)
        {
            return new StudentViewModel
            {
                Id = student.Id,
                Surname = student.Surname ?? string.Empty,
                Firstname = student.Firstname ?? string.Empty,
                Othername = student.Othername ?? string.Empty,
                Email = student.ApplicationUser?.Email ?? student.ApplicationUser?.UserName,
                GenderId = (int?)student.Gender,
                PassportPath = student.PaspportPath,
                ApplicationUserId = student.ApplicationUserId,
                IsActive = student.ApplicationUser?.LockoutEnd == null || student.ApplicationUser.LockoutEnd <= DateTimeOffset.UtcNow,
                CreatedDate = student.CreatedDate
            };
        }

        public async Task<StudentTable> GetStudentByIdAsync(string regnumber)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.ApplicationUser)
                    .FirstOrDefaultAsync(x => x.ApplicationUser.UserName == regnumber);

                if (student == null)
                    return null;

                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by registration number {RegNumber}", regnumber);
                return null;
            }
        }

        #endregion
    }
}
