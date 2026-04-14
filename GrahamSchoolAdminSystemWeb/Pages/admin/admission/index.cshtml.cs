using ClosedXML.Excel;
using GrahamSchoolAdminSystemAccess;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using GrahamSchoolAdminSystemWeb.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.admission
{
    [Authorize]
    [RequirePermission(SD.Permissions.VIEW)]
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<indexModel> _logger;

        [BindProperty]
        public StudentViewModel StudentModel { get; set; }

        public indexModel(IUnitOfWork unitOfWork, ILogger<indexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PageResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostCreateUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid form data";
                return Page();
            }

            try
            {
                if (StudentModel.Id > 0)
                {
                    // Update existing school class
                    var result = await _unitOfWork.StudentServices.UpdateStudentAsync(StudentModel);
                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Update",
                            entityType: "StudentTable",
                            entityId: StudentModel.Id.ToString(),
                            message: $"Student record '{StudentModel.Firstname} {StudentModel.Surname}' updated successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Student Name: {StudentModel.Firstname} {StudentModel.Surname}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
                else
                {
                    // Create new school class
                    var result = await _unitOfWork.StudentServices.CreateStudentAsync(StudentModel);

                    if (result.Succeeded)
                    {
                        await _unitOfWork.LogService.LogUserActionAsync(
                            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            userName: User.Identity?.Name,
                            action: "Create",
                            entityType: "StudentTable",
                            entityId: result.Data.ToString(),
                            message: $"Student Record '{StudentModel.Firstname} {StudentModel.Surname}' created successfully",
                            ipAddress: GetClientIpAddress(),
                            details: $"Student Name: {StudentModel.Firstname} {StudentModel.Surname}"
                        );

                        TempData["Success"] = result.Message;
                        return RedirectToPage();
                    }
                    TempData["Error"] = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Student admission");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student admission Processing Error",
                    message: "Error processing student admission",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );

                TempData["Error"] = "An error occurred while processing your request.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUploadExcelAsync(IFormFile ExcelFile)
        {
            try
            {
                if (!SD.IsExcelFile(ExcelFile) || !SD.IsExcelFileSecure(ExcelFile))
                {
                    TempData["Error"] = "Invalid or corrupted Excel file";
                }
                var result = await _unitOfWork.StudentServices.ImportStudentsFromExcelAsync(ExcelFile);
                if (result.Succeeded)
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name,
                        action: "Multiple Student Account",
                        entityType: "StudentTable",
                        entityId: "",
                        message: $"Multiple student accounts imported successfully",
                        ipAddress: GetClientIpAddress(),
                        details: $"Imported Excel File, Results: {string.Join(", ", result.Results.Select(r => r.ToString()))}"
                    );
                    TempData["Success"] = result.Message;
                    return RedirectToPage();
                }
                TempData["Error"] = result.Message;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing student records from Excel");
                await _unitOfWork.LogService.LogErrorAsync(
                    subject: "Student Import Error",
                    message: "Error importing student records from Excel",
                    details: ex.Message,
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    ipAddress: GetClientIpAddress()
                );
                TempData["Error"] = "An error occurred while processing your request.";
                return Page();
            }
        }

        public IActionResult OnPostDownloadExcel()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Students");

            // Set up headers
            worksheet.Cell(1, 1).Value = "Student Reg. Number *";
            worksheet.Cell(1, 2).Value = "Surname *";
            worksheet.Cell(1, 3).Value = "Firstname *";
            worksheet.Cell(1, 4).Value = "Othername";
            worksheet.Cell(1, 5).Value = "Gender(Male = 1 & Female = 2) *";
            worksheet.Cell(1, 6).Value = "PassportPath";

            // Format headers
            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add sample data (optional)
            worksheet.Cell(2, 1).Value = "john.doe@example.com";
            worksheet.Cell(2, 2).Value = "Doe";
            worksheet.Cell(2, 3).Value = "John";
            worksheet.Cell(2, 4).Value = "Michael";
            worksheet.Cell(2, 5).Value = "Male";
            worksheet.Cell(2, 6).Value = "/images/passports/johndoe.jpg";

            worksheet.Cell(3, 1).Value = "jane.smith@example.com";
            worksheet.Cell(3, 2).Value = "Smith";
            worksheet.Cell(3, 3).Value = "Jane";
            worksheet.Cell(3, 4).Value = "";
            worksheet.Cell(3, 5).Value = "Female";
            worksheet.Cell(3, 6).Value = "";

            // Add data validation for Gender column
            var genderColumn = worksheet.Column(5);
            genderColumn.SetDataValidation().List("Male,Female");

            // Add instructions sheet
            var instructionsSheet = workbook.Worksheets.Add("Instructions");
            instructionsSheet.Cell(1, 1).Value = "Student Import Template - Instructions";
            instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(1, 1).Style.Font.FontSize = 14;

            instructionsSheet.Cell(3, 1).Value = "Required Fields (marked with *)";
            instructionsSheet.Cell(3, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(4, 1).Value = "1. Email - Must be unique";
            instructionsSheet.Cell(5, 1).Value = "2. Surname - Student's last name";
            instructionsSheet.Cell(6, 1).Value = "3. Firstname - Student's first name";
            instructionsSheet.Cell(7, 1).Value = "4. Gender - Enter 'Male' or 'Female'";

            instructionsSheet.Cell(9, 1).Value = "Optional Fields";
            instructionsSheet.Cell(9, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(10, 1).Value = "1. Othername - Middle name or other names";
            instructionsSheet.Cell(11, 1).Value = "2. PassportPath - Path to student's passport photo";

            instructionsSheet.Cell(13, 1).Value = "Notes:";
            instructionsSheet.Cell(13, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(14, 1).Value = "- Default password for all imported accounts is: 12345678";
            instructionsSheet.Cell(15, 1).Value = "- Sample data is provided in rows 2-3 of the Students sheet";
            instructionsSheet.Cell(16, 1).Value = "- Delete sample data before importing your students";
            instructionsSheet.Cell(17, 1).Value = "- Do not modify the header row (row 1)";

            instructionsSheet.Columns().AdjustToContents();
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Student_Import_Template.xlsx");
        }

        private string GetClientIpAddress()
        {
            try
            {
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                    return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0];
                return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
