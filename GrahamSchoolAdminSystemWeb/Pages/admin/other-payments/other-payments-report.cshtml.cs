using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.DTOs;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments
{
    public class other_payments_reportModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty(SupportsGet = true)]
        public int? SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Term { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ClassId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PaymentItemId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PaymentStatus { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string PaymentState { get; set; } = "all";

        public List<FeesPaymentsDto> RawReportData { get; set; } = new();
        public List<dynamic> AggregatedReportData { get; set; } = new();
        public ViewSelections Selections { get; set; } = new();

        // Summary properties
        public int TotalRecords { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CollectionPercentage { get; set; }

        public other_payments_reportModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadSelectionsAsync();
                await LoadReportDataAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading report: {ex.Message}";
                RawReportData = new List<FeesPaymentsDto>();
                AggregatedReportData = new List<dynamic>();
            }
        }

        public async Task<IActionResult> OnGetExportAsync(string format = "csv")
        {
            try
            {
                await LoadReportDataAsync();

                return format?.ToLower() switch
                {
                    "excel" => ExportToExcel(RawReportData),
                    "pdf" => ExportToPdf(RawReportData),
                    _ => File(Encoding.UTF8.GetBytes(ExportToCSV(RawReportData)),
                        "text/csv",
                        $"other-payments-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv")
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error exporting report: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadSelectionsAsync()
        {
            try
            {
                Selections = await _unitOfWork.FinanceServices.GetOtherFeesSetupSelectionsAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading filter options: {ex.Message}";
                Selections = new ViewSelections();
            }
        }

        private async Task LoadReportDataAsync()
        {
            try
            {
                // Retrieve all payment data with pagination (large page size for report)
                var (data, total, filtered) = await _unitOfWork.OtherPaymentServices.GetOtherPaymentsAsync(
                    skip: 0,
                    pageSize: 10000, // Large page size to get all data for report
                    searchTerm: "",
                    sortColumn: 5,
                    sortDirection: "desc",
                    termFilter: Term,
                    sessionFilter: SessionId,
                    classFilter: ClassId,
                    subclassFilter: null
                );

                // Apply additional filtering if needed
                RawReportData = data
                    .Where(x =>
                    {
                        // Filter by payment status if specified
                        if (PaymentStatus != "all" && !x.status.Contains(PaymentStatus))
                            return false;

                        // Filter by payment state if specified
                        if (PaymentState != "all" && !x.state.Contains(PaymentState))
                            return false;

                        // Filter by payment item if specified
                        if (PaymentItemId.HasValue && x.paymentitemid != PaymentItemId)
                            return false;

                        return true;
                    })
                    .ToList();

                // Aggregate data by Session/Term/Class/PaymentItem
                AggregateReportData();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading report data: {ex.Message}";
                RawReportData = new List<FeesPaymentsDto>();
                AggregatedReportData = new List<dynamic>();
            }
        }

        private void AggregateReportData()
        {
            if (!RawReportData.Any())
            {
                AggregatedReportData = new List<dynamic>();
                return;
            }

            // Group by Session, Term, Class, and PaymentItem
            var grouped = RawReportData
                .GroupBy(x => new { x.session, x.term, x.schoolclass, x.paymentitem })
                .Select(g => new
                {
                    g.Key.session,
                    g.Key.term,
                    g.Key.schoolclass,
                    g.Key.paymentitem,
                    StudentCount = g.Select(x => x.regnumber).Distinct().Count(),
                    FeeAmountPerStudent = g.FirstOrDefault()?.fees ?? "₦0.00",
                    TotalExpected = g.Sum(x => decimal.Parse(x.fees.Replace("₦", "").Replace(",", ""))),
                    TotalPaid = g.Sum(x => decimal.Parse(x.amount.Replace("₦", "").Replace(",", ""))),
                    TotalOutstanding = g.Sum(x => decimal.Parse(x.balance.Replace("₦", "").Replace(",", "")))
                })
                .ToList();

            // Convert to dynamic list and calculate percentages
            AggregatedReportData = grouped.Select(x => new
            {
                Session = x.session,
                Term = x.term,
                ClassName = x.schoolclass,
                PaymentItem = x.paymentitem,
                StudentCount = x.StudentCount,
                FeePerStudent = x.FeeAmountPerStudent,
                ExpectedAmount = x.TotalExpected,
                PaidAmount = x.TotalPaid,
                OutstandingAmount = x.TotalOutstanding,
                CollectionPercentage = x.TotalExpected > 0 
                    ? Math.Round((x.TotalPaid / x.TotalExpected) * 100, 2) 
                    : 0
            }).Cast<dynamic>().ToList();

            // Calculate summary statistics
            TotalRecords = RawReportData.Count;
            TotalExpected = AggregatedReportData.Cast<dynamic>()
                .Sum(x => (decimal)x.ExpectedAmount);
            TotalCollected = AggregatedReportData.Cast<dynamic>()
                .Sum(x => (decimal)x.PaidAmount);
            TotalOutstanding = TotalExpected - TotalCollected;
            CollectionPercentage = TotalExpected > 0 
                ? Math.Round((TotalCollected / TotalExpected) * 100, 2) 
                : 0;
        }

        private IActionResult ExportToExcel(List<FeesPaymentsDto> reportData)
        {
            try
            {
                var html = BuildExcelHtml(reportData);
                var bytes = Encoding.UTF8.GetBytes(html);

                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"other-payments-report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating Excel: {ex.Message}";
                return RedirectToPage();
            }
        }

        private IActionResult ExportToPdf(List<FeesPaymentsDto> reportData)
        {
            try
            {
                var pdfContent = BuildPdfContent(reportData);
                var bytes = Encoding.UTF8.GetBytes(pdfContent);

                return File(bytes,
                    "application/pdf",
                    $"other-payments-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating PDF: {ex.Message}";
                return RedirectToPage();
            }
        }

        private string BuildExcelHtml(List<FeesPaymentsDto> reportData)
        {
            var html = new StringBuilder();
            html.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            html.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            html.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            html.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            html.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            html.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            html.AppendLine("<Worksheet ss:Name=\"Other Payments\">");
            html.AppendLine("<Table>");

            // Headers
            html.AppendLine("<Row ss:StyleID=\"Header\">");
            var headers = new[] { "Student Name", "Reg Number", "Class", "Session", "Term", "Payment Item", "Item Amount", "Amount Paid", "Balance", "Status", "Payment State", "Date" };
            foreach (var header in headers)
            {
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{header}</Data></Cell>");
            }
            html.AppendLine("</Row>");

            // Data rows
            decimal totalAmount = 0;
            decimal totalPaid = 0;
            decimal totalBalance = 0;

            foreach (var item in reportData)
            {
                html.AppendLine("<Row>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.name}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.regnumber}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.schoolclass}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.session}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.term}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.paymentitem}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.fees}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.amount}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.balance}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{ExtractStatus(item.status)}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{ExtractState(item.state)}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.createdate:yyyy-MM-dd HH:mm}</Data></Cell>");
                html.AppendLine("</Row>");
            }

            html.AppendLine("</Table>");
            html.AppendLine("</Worksheet>");
            html.AppendLine("</Workbook>");

            return html.ToString();
        }

        private string BuildPdfContent(List<FeesPaymentsDto> reportData)
        {
            var report = new StringBuilder();

            report.AppendLine("OTHER PAYMENTS REPORT");
            report.AppendLine("===============================================");
            report.AppendLine();
            report.AppendLine($"Generated: {DateTime.Now:dddd, MMMM dd, yyyy HH:mm:ss}");
            report.AppendLine();

            if (SessionId.HasValue || Term.HasValue || ClassId.HasValue)
            {
                report.AppendLine("FILTERS APPLIED:");
                if (SessionId.HasValue)
                    report.AppendLine($"  Session: {SessionId}");
                if (Term.HasValue)
                    report.AppendLine($"  Term: {Term}");
                if (ClassId.HasValue)
                    report.AppendLine($"  Class: {ClassId}");
                report.AppendLine();
            }

            report.AppendLine("SUMMARY");
            report.AppendLine("-------");
            report.AppendLine($"Total Records: {reportData.Count}");
            report.AppendLine();
            report.AppendLine("REPORT DETAILS");
            report.AppendLine("===============================================");
            report.AppendLine();

            // Headers with better formatting
            report.AppendLine(string.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-20} {5,-15}",
                "Student Name", "Reg Number", "Class", "Session", "Payment Item", "Amount Paid"));
            report.AppendLine(new string('-', 120));

            // Data rows
            foreach (var item in reportData)
            {
                report.AppendLine(string.Format("{0,-25} {1,-15} {2,-15} {3,-15} {4,-20} {5,-15}",
                    item.name, item.regnumber, item.schoolclass, item.session, item.paymentitem, item.amount));
            }

            report.AppendLine(new string('=', 120));
            report.AppendLine($"Total Payments Recorded: {reportData.Count}");
            report.AppendLine();
            report.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            return report.ToString();
        }

        private string ExportToCSV(List<FeesPaymentsDto> reportData)
        {
            var csv = new StringBuilder();

            // Headers
            csv.AppendLine("Student Name,Reg Number,Class,Session,Term,Payment Item,Item Amount,Amount Paid,Balance,Status,Payment State,Date");

            // Data rows
            foreach (var item in reportData)
            {
                csv.AppendLine($"\"{item.name}\",\"{item.regnumber}\",\"{item.schoolclass}\"," +
                    $"\"{item.session}\",\"{item.term}\",\"{item.paymentitem}\"," +
                    $"{item.fees},{item.amount},{item.balance}," +
                    $"\"{ExtractStatus(item.status)}\",\"{ExtractState(item.state)}\"," +
                    $"{item.createdate:yyyy-MM-dd HH:mm}");
            }

            return csv.ToString();
        }

        private string ExtractStatus(string htmlStatus)
        {
            // Remove HTML tags from status badge
            if (string.IsNullOrEmpty(htmlStatus))
                return "Unknown";

            return htmlStatus
                .Replace("<span class=\"badge bg-success\">", "")
                .Replace("<span class=\"badge bg-warning\">", "")
                .Replace("<span class=\"badge bg-danger\">", "")
                .Replace("</span>", "")
                .Trim();
        }

        private string ExtractState(string htmlState)
        {
            // Remove HTML tags from state badge
            if (string.IsNullOrEmpty(htmlState))
                return "Unknown";

            return htmlState
                .Replace("<span class=\"badge bg-info\">", "")
                .Replace("<span class=\"badge bg-success\">", "")
                .Replace("<span class=\"badge bg-warning\">", "")
                .Replace("</span>", "")
                .Trim();
        }
    }
}
