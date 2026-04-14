using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IStudentServices
    {
        Task<(List<StudentViewModel> data, int recordsTotal, int recordsFiltered)> GetStudentsAsync(
            int start = 0, 
            int length = 10, 
            string searchValue = "", 
            int sortColumnIndex = 0, 
            string sortDirection = "asc");

        Task<StudentViewModel> GetStudentByIdAsync(int studentId);
        Task<StudentTable> GetStudentByIdAsync(string regnumber);

        Task<(bool Succeeded, string Message, object Data)> CreateStudentAsync(StudentViewModel model);

        Task<(bool Succeeded, string Message)> UpdateStudentAsync(StudentViewModel model);

        Task<(bool Succeeded, string Message, List<StudentImportResult> Results)> ImportStudentsFromExcelAsync(IFormFile excelFile);

        Task<(bool Succeeded, string Message)> DeleteStudentAsync(int studentId);

        Task<(bool Succeeded, string Message)> ToggleStudentActivationAsync(int studentId);
    }
}
