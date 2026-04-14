using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrahamSchoolAdminSystemWeb.Controllers
{
    [Authorize]
    public class homeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<homeController> _logger;

        public homeController(IUnitOfWork unitOfWork, ILogger<homeController> logger)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> getschoolclasses()
        {
            return await ExecuteDataTableAsync<SchoolClassesDto>( _unitOfWork.SystemActivities.GetSchoolClassesAsync,
                "Error retrieving categories");
        }

        [HttpPost]
        public async Task<IActionResult> GetSchoolClassesDataTable()
        {

            try
            {
                return await ExecuteDataTableAsync<SchoolClassesDto>(_unitOfWork.SystemActivities.GetSchoolClassesAsync,
                "Error retrieving categories");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading school classes DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAcademicSessionsDataTable()
        {
            try
            {
                return await ExecuteDataTableAsync<SessionYearDto>(_unitOfWork.SystemActivities.GetAcademicSessionAsync,
                    "Error retrieving academic sessions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading academic sessions DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSchoolSubClassesDataTable()
        {
            try
            {
                return await ExecuteDataTableAsync<SchoolSubClassDto>(_unitOfWork.SystemActivities.GetSchoolSubClassesAsync,
                    "Error retrieving school sub-classes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading school sub-classes DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetStudentsDataTable()
        {
            try
            {
                return await ExecuteDataTableAsync<StudentViewModel>(_unitOfWork.StudentServices.GetStudentsAsync,
                    "Error retrieving students");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetTermRegistrationsDataTable()
        {
            try
            {
                var request = new DataTableRequest
                {
                    Draw = int.TryParse(Request.Form["draw"].FirstOrDefault(), out var d) ? d : 0,
                    Start = string.IsNullOrEmpty(Request.Form["start"].FirstOrDefault()) ? 0 : Convert.ToInt32(Request.Form["start"].FirstOrDefault()),
                    Length = string.IsNullOrEmpty(Request.Form["length"].FirstOrDefault()) ? 0 : Convert.ToInt32(Request.Form["length"].FirstOrDefault()),
                    SortColumn = 0,
                    SortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc",
                    SearchValue = Request.Form["search[value]"].FirstOrDefault()
                };

                // Extract filter parameters if they exist
                int? termFilter = null;
                int? sessionFilter = null;
                int? classFilter = null;
                int? subclassFilter = null;

                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;
                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("schoolclass") && int.TryParse(Request.Form["schoolclass"].FirstOrDefault(), out int schoolclass) && schoolclass > 0)
                    classFilter = schoolclass;
                if (Request.Form.ContainsKey("subclass") && int.TryParse(Request.Form["subclass"].FirstOrDefault(), out int subclass) && subclass > 0)
                    subclassFilter = subclass;

                var (data, recordsTotal, recordsFiltered) = await _unitOfWork.TermRegistrationServices.GetStudentTermRegistrationAsync(
                    skip: request.Start ?? 0,
                    pageSize: request.Length ?? 10,
                    searchTerm: request.SearchValue ?? "",
                    sortColumn: request.SortColumn,
                    sortDirection: request.SortDirection ?? "asc",
                    termFilter: termFilter,
                    sessionFilter: sessionFilter,
                    classFilter: classFilter,
                    subclassFilter: subclassFilter
                );

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading term registrations DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeesDataTable()
        {
            try
            {
                return await ExecuteDataTableAsync<EmployeeViewModel>(_unitOfWork.UsersServices.GetEmployeesAsync,
                    "Error retrieving employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentCategoriesDataTable()
        {
            try
            {
                return await ExecuteDataTableAsync<dynamic>(_unitOfWork.PaymentCategoryService.GetPaymentCategoriesAsync,
                    "Error retrieving payment categories");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment categories DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentItemsDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? categoryFilter = null;
                if (Request.Form.ContainsKey("category") && int.TryParse(Request.Form["category"].FirstOrDefault(), out int cat) && cat > 0)
                    categoryFilter = cat;

                var (data, recordsTotal, recordsFiltered) = await _unitOfWork.PaymentItemService.GetPaymentItemsAsync(
                    skip: request.Start ?? 0,
                    pageSize: request.Length ?? 10,
                    searchTerm: request.SearchValue ?? "",
                    sortColumn: request.SortColumn,
                    sortDirection: request.SortDirection ?? "asc",
                    categoryFilter: categoryFilter
                );

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment items DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPaymentSetupsDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? sessionFilter = null;
                int? termFilter = null;
                int? classFilter = null;

                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;
                if (Request.Form.ContainsKey("schoolclass") && int.TryParse(Request.Form["schoolclass"].FirstOrDefault(), out int cls) && cls > 0)
                    classFilter = cls;

                var (data, recordsTotal, recordsFiltered) = await _unitOfWork.PaymentSetupService.GetPaymentSetupsAsync(
                    skip: request.Start ?? 0,
                    pageSize: request.Length ?? 10,
                    searchTerm: request.SearchValue ?? "",
                    sortColumn: request.SortColumn,
                    sortDirection: request.SortDirection ?? "asc",
                    sessionFilter: sessionFilter,
                    termFilter: termFilter,
                    classFilter: classFilter
                );

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment setups DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetStudentPaymentsDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? sessionFilter = null;
                int? termFilter = null;
                int? classFilter = null;
                string? statusFilter = null;
                int? stateFilter = null;

                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;
                if (Request.Form.ContainsKey("schoolclass") && int.TryParse(Request.Form["schoolclass"].FirstOrDefault(), out int cls) && cls > 0)
                    classFilter = cls;
                if (Request.Form.ContainsKey("status") && !string.IsNullOrWhiteSpace(Request.Form["status"].FirstOrDefault()))
                    statusFilter = Request.Form["status"].FirstOrDefault();
                if (Request.Form.ContainsKey("state") && int.TryParse(Request.Form["state"].FirstOrDefault(), out int st) && st > 0)
                    stateFilter = st;

                var (data, recordsTotal, recordsFiltered) = await _unitOfWork.StudentPaymentService.GetPaymentsDataTableAsync(
                    skip: request.Start ?? 0,
                    pageSize: request.Length ?? 10,
                    searchTerm: request.SearchValue ?? "",
                    sortColumn: request.SortColumn,
                    sortDirection: request.SortDirection ?? "asc",
                    sessionFilter: sessionFilter,
                    termFilter: termFilter,
                    classFilter: classFilter,
                    statusFilter: statusFilter,
                    stateFilter: stateFilter
                );

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = recordsFiltered,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student payments DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetClassReportDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? sessionFilter = null, termFilter = null, classFilter = null,
                     categoryFilter = null, subclassFilter = null, paymentItemFilter = null;

                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;
                if (Request.Form.ContainsKey("schoolclass") && int.TryParse(Request.Form["schoolclass"].FirstOrDefault(), out int cls) && cls > 0)
                    classFilter = cls;
                if (Request.Form.ContainsKey("subclass") && int.TryParse(Request.Form["subclass"].FirstOrDefault(), out int sub) && sub > 0)
                    subclassFilter = sub;
                if (Request.Form.ContainsKey("category") && int.TryParse(Request.Form["category"].FirstOrDefault(), out int cat) && cat > 0)
                    categoryFilter = cat;
                if (Request.Form.ContainsKey("paymentitem") && int.TryParse(Request.Form["paymentitem"].FirstOrDefault(), out int item) && item > 0)
                    paymentItemFilter = item;

                if (!sessionFilter.HasValue)
                    return new JsonResult(new { draw = request.Draw, recordsFiltered = 0, recordsTotal = 0, data = new List<object>(), summary = new { } });

                var result = await _unitOfWork.PaymentReportService.GetClassReportAsync(
                    sessionFilter.Value, termFilter, classFilter, subclassFilter, categoryFilter, paymentItemFilter,
                    request.Start ?? 0, 200, request.SearchValue ?? "",
                    request.SortColumn, request.SortDirection ?? "asc");

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = result.RecordsFiltered,
                    recordsTotal = result.RecordsTotal,
                    data = result.Rows,
                    summary = result.Summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading class report DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSchoolReportDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? sessionFilter = null, termFilter = null;

                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;

                if (!sessionFilter.HasValue || !termFilter.HasValue)
                    return new JsonResult(new { draw = request.Draw, recordsFiltered = 0, recordsTotal = 0, data = new List<object>(), summary = new { } });

                var result = await _unitOfWork.PaymentReportService.GetSchoolReportAsync(
                    sessionFilter.Value, termFilter.Value,
                    request.Start ?? 0, 200, request.SearchValue ?? "",
                    request.SortColumn, request.SortDirection ?? "asc");

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = result.RecordsFiltered,
                    recordsTotal = result.RecordsTotal,
                    data = result.Rows,
                    summary = result.Summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading school report DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryItemReportDataTable()
        {
            try
            {
                var request = ParseRequest();

                int? sessionFilter = null, termFilter = null, categoryFilter = null, classFilter = null;

                if (Request.Form.ContainsKey("session") && int.TryParse(Request.Form["session"].FirstOrDefault(), out int session) && session > 0)
                    sessionFilter = session;
                if (Request.Form.ContainsKey("term") && int.TryParse(Request.Form["term"].FirstOrDefault(), out int term) && term > 0)
                    termFilter = term;
                if (Request.Form.ContainsKey("category") && int.TryParse(Request.Form["category"].FirstOrDefault(), out int cat) && cat > 0)
                    categoryFilter = cat;
                if (Request.Form.ContainsKey("schoolclass") && int.TryParse(Request.Form["schoolclass"].FirstOrDefault(), out int cls) && cls > 0)
                    classFilter = cls;

                if (!sessionFilter.HasValue || !termFilter.HasValue)
                    return new JsonResult(new { draw = request.Draw, recordsFiltered = 0, recordsTotal = 0, data = new List<object>(), summary = new { } });

                var result = await _unitOfWork.PaymentReportService.GetCategoryItemReportAsync(
                    sessionFilter.Value, termFilter.Value, categoryFilter, classFilter,
                    request.Start ?? 0, 200, request.SearchValue ?? "",
                    request.SortColumn, request.SortDirection ?? "asc");

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = result.RecordsFiltered,
                    recordsTotal = result.RecordsTotal,
                    data = result.Rows,
                    summary = result.Summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category item report DataTable");
                return Json(new { error = "Error loading data" });
            }
        }

        private async Task<IActionResult> ExecuteDataTableAsync<T>(Func<int, int, string, int, string, Task<(List<T> data, int recordsTotal, int recordsFiltered)>> serviceCall, string errorMessage = "An error occurred")
        {
            var request = ParseRequest();

            try
            {
                var (data, recordsTotal, recordsFiltered) = await serviceCall(
                    request.Start ?? 0,
                    request.Length ?? 10,
                    request.SearchValue ?? "",
                    request.SortColumn,
                    request.SortDirection ?? "asc");

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered,
                    recordsTotal,
                    data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);

                return new JsonResult(new
                {
                    draw = request.Draw,
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<T>(),
                    error = errorMessage
                });
            }
        }

        private DataTableRequest ParseRequest()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();

            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            return new DataTableRequest
            {
                Draw = int.TryParse(draw, out var d) ? d : 0,
                Start = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start),
                Length = string.IsNullOrEmpty(length) ? 0 : Convert.ToInt32(length),
                SortColumn = string.IsNullOrWhiteSpace(sortColumn) ? 0 : Convert.ToInt32(sortColumn),
                SortDirection = sortDirection,
                SearchValue = searchValue
            };
        }
    }
}
