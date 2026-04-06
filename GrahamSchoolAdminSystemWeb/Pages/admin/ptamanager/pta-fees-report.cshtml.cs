using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.ptamanager
{
    public class PtaFeesReportModel : PageModel
    {
        private readonly IPTAPaymentServices _ptaPaymentServices;

        [BindProperty(SupportsGet = true)]
        public int? SessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Term { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ClassId { get; set; }

        public PTAFeesReportViewModel ReportData { get; set; } = new();

        public PtaFeesReportModel(IPTAPaymentServices ptaPaymentServices)
        {
            _ptaPaymentServices = ptaPaymentServices;
        }

        public async Task OnGetAsync()
        {
            try
            {
                ReportData = await _ptaPaymentServices.GetPTAFeesReportAsync(SessionId, Term, ClassId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading report: {ex.Message}";
                ReportData = new PTAFeesReportViewModel();
            }
        }

        public async Task<IActionResult> OnGetExportAsync(string format = "csv")
        {
            try
            {
                var reportData = await _ptaPaymentServices.GetPTAFeesReportAsync(SessionId, Term, ClassId);

                return format?.ToLower() switch
                {
                    "excel" => ExportToExcel(reportData),
                    "pdf" => ExportToPdf(reportData),
                    _ => File(Encoding.UTF8.GetBytes(ExportToCSV(reportData)), 
                        "text/csv", 
                        $"pta-fees-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv")
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error exporting report: {ex.Message}";
                return RedirectToPage();
            }
        }

        private IActionResult ExportToExcel(PTAFeesReportViewModel reportData)
        {
            try
            {
                var html = BuildExcelHtml(reportData);
                var bytes = Encoding.UTF8.GetBytes(html);

                return File(bytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"pta-fees-report-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating Excel: {ex.Message}";
                return RedirectToPage();
            }
        }

        private IActionResult ExportToPdf(PTAFeesReportViewModel reportData)
        {
            try
            {
                var pdfContent = BuildPdfContent(reportData);
                var bytes = Encoding.UTF8.GetBytes(pdfContent);

                return File(bytes, 
                    "application/pdf", 
                    $"pta-fees-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating PDF: {ex.Message}";
                return RedirectToPage();
            }
        }

        private string BuildExcelHtml(PTAFeesReportViewModel reportData)
        {
            var html = new StringBuilder();
            html.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            html.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            html.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            html.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            html.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            html.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            html.AppendLine("<Worksheet ss:Name=\"PTA Fees Report\">");
            html.AppendLine("<Table>");

            // Headers
            html.AppendLine("<Row ss:StyleID=\"Header\">");
            var headers = new[] { "Session", "Term", "Class", "No. of Students", "Fee per Student", "Expected Amount", "Actual Paid", "Outstanding", "Collection %" };
            foreach (var header in headers)
            {
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{header}</Data></Cell>");
            }
            html.AppendLine("</Row>");

            // Data rows
            foreach (var item in reportData.ReportData)
            {
                html.AppendLine("<Row>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.Session}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.TermName}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.ClassName}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"Number\">{item.StudentCount}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"Number\">{item.FeeAmountPerStudent:N2}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"Number\">{item.ExpectedAmount:N2}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"Number\">{item.ActualAmount:N2}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"Number\">{item.OutstandingAmount:N2}</Data></Cell>");
                html.AppendLine($"<Cell><Data ss:Type=\"String\">{item.CollectionPercentage:N2}%</Data></Cell>");
                html.AppendLine("</Row>");
            }

            // Totals
            html.AppendLine("<Row ss:StyleID=\"Total\">");
            html.AppendLine($"<Cell><Data ss:Type=\"String\">TOTALS</Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"String\"></Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"String\"></Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"Number\">{reportData.TotalStudents}</Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"String\"></Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"Number\">{reportData.TotalExpected:N2}</Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"Number\">{reportData.TotalActual:N2}</Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"Number\">{reportData.TotalOutstanding:N2}</Data></Cell>");
            html.AppendLine($"<Cell><Data ss:Type=\"String\">{reportData.OverallCollectionPercentage:N2}%</Data></Cell>");
            html.AppendLine("</Row>");

            html.AppendLine("</Table>");
            html.AppendLine("</Worksheet>");
            html.AppendLine("</Workbook>");

            return html.ToString();
        }

        private string BuildPdfContent(PTAFeesReportViewModel reportData)
        {
            var report = new StringBuilder();

            report.AppendLine("PTA FEES PAYMENT REPORT");
            report.AppendLine("===============================================");
            report.AppendLine();
            report.AppendLine($"Generated: {DateTime.Now:dddd, MMMM dd, yyyy HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("SUMMARY");
            report.AppendLine("-------");
            report.AppendLine($"Total Students: {reportData.TotalStudents}");
            report.AppendLine($"Expected Amount: ₦{reportData.TotalExpected:N2}");
            report.AppendLine($"Actual Paid: ₦{reportData.TotalActual:N2}");
            report.AppendLine($"Outstanding: ₦{reportData.TotalOutstanding:N2}");
            report.AppendLine($"Collection Rate: {reportData.OverallCollectionPercentage:N2}%");
            report.AppendLine();
            report.AppendLine("REPORT DETAILS");
            report.AppendLine("===============================================");
            report.AppendLine();

            // Headers
            report.AppendLine(string.Format("{0,-15} {1,-12} {2,-20} {3,12} {4,18} {5,15}", 
                "Session", "Term", "Class", "Students", "Actual Paid", "Collection %"));
            report.AppendLine(new string('-', 110));

            // Data rows
            foreach (var item in reportData.ReportData)
            {
                report.AppendLine(string.Format("{0,-15} {1,-12} {2,-20} {3,12} ₦{4,16:N2} {5,14:N2}%", 
                    item.Session, item.TermName, item.ClassName, item.StudentCount, item.ActualAmount, item.CollectionPercentage));
            }

            report.AppendLine(new string('=', 110));
            report.AppendLine(string.Format("{0,-15} {1,-12} {2,-20} {3,12} ₦{4,16:N2} {5,14:N2}%", 
                "TOTALS", "", "", reportData.TotalStudents, reportData.TotalActual, reportData.OverallCollectionPercentage));
            report.AppendLine();
            report.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            return report.ToString();
        }

        private string ExportToCSV(PTAFeesReportViewModel reportData)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Session,Term,Class,No. of Students,Fee per Student,Expected Amount,Actual Paid,Outstanding,Collection %");

            foreach (var item in reportData.ReportData)
            {
                csv.AppendLine($"\"{item.Session}\",\"{item.TermName}\",\"{item.ClassName}\",{item.StudentCount},{item.FeeAmountPerStudent:N2},{item.ExpectedAmount:N2},{item.ActualAmount:N2},{item.OutstandingAmount:N2},{item.CollectionPercentage:N2}");
            }

            csv.AppendLine($"TOTALS,,,,{reportData.TotalExpected:N2},{reportData.TotalActual:N2},{reportData.TotalOutstanding:N2},{reportData.OverallCollectionPercentage:N2}");

            return csv.ToString();
        }
    }
}
