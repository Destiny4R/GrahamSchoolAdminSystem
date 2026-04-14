using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemAccess.ServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GrahamSchoolAdminSystemWeb.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IViewsSelectionOptions _viewsSelectionOptions;

        // User info for dashboard greeting
        public UserDisplayInfo CurrentUser { get; set; } = new();

        // Current settings info
        public string CurrentSessionName { get; set; } = "N/A";
        public string CurrentTermName { get; set; } = "N/A";
        public int CurrentSessionId { get; set; }
        public int CurrentTerm { get; set; }
        public bool SettingsConfigured { get; set; }

        // Summary totals
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public int TotalStudentsRegistered { get; set; }
        public int TotalCategories { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            IPermissionService permissionService,
            IUnitOfWork unitOfWork,
            IViewsSelectionOptions viewsSelectionOptions)
        {
            _logger = logger;
            _permissionService = permissionService;
            _unitOfWork = unitOfWork;
            _viewsSelectionOptions = viewsSelectionOptions;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    CurrentUser = await _permissionService.GetUserDisplayInfoAsync(userId);
                }

                var settings = await _unitOfWork.UsersServices.GetAppSettingsByUserIdAsync();
                if (settings != null && settings.sessionId > 0 && settings.term > 0)
                {
                    SettingsConfigured = true;
                    CurrentSessionId = settings.sessionId;
                    CurrentTerm = settings.term;
                    CurrentTermName = ((GrahamSchoolAdminSystemModels.Models.GetEnums.Term)settings.term).ToString();

                    // Resolve session name from dropdown list
                    var sessionItems = await _viewsSelectionOptions.GetSessionsForDropdownAsync();
                    CurrentSessionName = sessionItems
                        .FirstOrDefault(s => s.Value == settings.sessionId.ToString())?.Text ?? "N/A";

                    // Load summary cards data
                    var categorySummary = await _unitOfWork.PaymentReportService
                        .GetDashboardCategorySummaryAsync(settings.sessionId, settings.term);
                    TotalExpected = categorySummary.Sum(c => c.Expected);
                    TotalCollected = categorySummary.Sum(c => c.Collected);
                    TotalCategories = categorySummary.Count;

                    var termRegChart = await _unitOfWork.PaymentReportService
                        .GetDashboardTermRegistrationChartAsync(settings.sessionId);
                    TotalStudentsRegistered = termRegChart.Counts.Sum();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }
        }
    }
}
