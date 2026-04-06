using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.feesmanager.fees_payment
{
    public class fees_payment_receiptModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentInvoiceViewModel Invoice { get; set; } = new();

        public fees_payment_receiptModel(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int paymentId)
        {
            try
            {
                // Fetch payment record with all related data
                var payment = await _unitOfWork.FeesPaymentServices.GetPaymentByIdAsync(paymentId);

                if (payment == null)
                {
                    TempData["Error"] = "Payment record not found.";
                    return RedirectToPage("index");
                }

                // Map to invoice view model
                Invoice = await BuildInvoiceViewModelAsync(payment);

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading invoice: {ex.Message}";
                return RedirectToPage("index");
            }
        }

        /// <summary>
        /// Download invoice as standalone HTML file
        /// </summary>
        public async Task<IActionResult> OnGetDownloadAsync(int paymentId)
        {
            try
            {
                // Fetch payment record with all related data
                var payment = await _unitOfWork.FeesPaymentServices.GetPaymentByIdAsync(paymentId);

                if (payment == null)
                {
                    return NotFound("Payment record not found.");
                }

                // Build invoice view model
                var invoice = await BuildInvoiceViewModelAsync(payment);

                // Generate standalone HTML
                string htmlContent = GenerateStandaloneHtmlInvoice(invoice);

                // Return as HTML file download
                byte[] fileBytes = Encoding.UTF8.GetBytes(htmlContent);
                string fileName = $"Invoice_{invoice.InvoiceNumber.Replace("-", "_")}.html";

                return File(fileBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating invoice: {ex.Message}");
            }
        }

        /// <summary>
        /// Build invoice view model from payment data
        /// </summary>
        private async Task<PaymentInvoiceViewModel> BuildInvoiceViewModelAsync(dynamic payment)
        {
            var totalPreviousPayments = await _unitOfWork.FeesPaymentServices.GetTotalPreviousPaymentsAsync(payment.TermRegId, payment.Id);

            return new PaymentInvoiceViewModel
            {
                InvoiceId = payment.Id,
                InvoiceNumber = payment.InvoiceNumber,
                InvoiceDate = payment.CreatedDate,
                PaymentReference = GeneratePaymentReference(payment.Id, payment.TermRegId),

                // Student Information
                StudentName = payment.TermRegistration?.Student?.FullName ?? "N/A",
                StudentRegNumber = payment.TermRegistration?.Student?.ApplicationUser?.UserName ?? "N/A",
                StudentClass = payment.TermRegistration?.SchoolClass?.Name ?? "N/A",

                // Academic Information
                Term = payment.TermRegistration?.Term.ToString() ?? "N/A",
                Session = payment.TermRegistration?.SessionYear?.Name ?? "N/A",

                // Payment Details
                TotalFees = payment.Fees,
                PreviousPayments = totalPreviousPayments,
                PreviousBalance = await _unitOfWork.FeesPaymentServices.GetPreviousBalanceAsync(payment.TermRegId),
                CurrentPayment = payment.Amount,
                NewBalance = payment.Fees - (totalPreviousPayments + payment.Amount),

                // Payment Status
                PaymentStatus = payment.Status.ToString(),
                PaymentState = payment.PaymentState.ToString(),
                PaymentDate = payment.CreatedDate,

                // Evidence
                EvidenceFileName = System.IO.Path.GetFileName(payment.FilePath),
                EvidenceFilePath = payment.FilePath,

                // Additional Information
                Narration = payment.Narration ?? "No additional notes",
                School = "Graham School",
                SchoolContactInfo = "Address: [Your School Address] | Phone: [Your Phone] | Email: [Your Email]"
            };
        }

        /// <summary>
        /// Generate a complete standalone HTML invoice
        /// </summary>
        private string GenerateStandaloneHtmlInvoice(PaymentInvoiceViewModel invoice)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>Invoice {invoice.InvoiceNumber}</title>");
            html.AppendLine("    <style>");
            html.AppendLine(GetInvoiceCss());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"invoice-container\">");

            // Header
            html.AppendLine("        <div class=\"invoice-header\">");
            html.AppendLine("            <div class=\"company-info\">");
            html.AppendLine("                <h2>🏢 " + invoice.School + "</h2>");
            html.AppendLine($"                <p>{invoice.SchoolContactInfo}</p>");
            html.AppendLine("                <p>Payment Management System</p>");
            html.AppendLine("            </div>");
            html.AppendLine("            <div class=\"invoice-info\">");
            html.AppendLine("                <p><span class=\"invoice-info-label\">Invoice #:</span><br>");
            html.AppendLine($"                <span class=\"invoice-number-display\">{invoice.InvoiceNumber}</span></p>");
            html.AppendLine("                <p><span class=\"invoice-info-label\">Invoice Date:</span><br>");
            html.AppendLine($"                <span>{invoice.InvoiceDate:dd MMMM yyyy}</span></p>");
            html.AppendLine("                <p><span class=\"invoice-info-label\">Payment Reference:</span><br>");
            html.AppendLine($"                <span class=\"payment-reference mt-2\">{invoice.PaymentReference}</span></p>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");

            // Student Information
            html.AppendLine("        <div class=\"invoice-section\">");
            html.AppendLine("            <h3>👤 Student Information</h3>");
            html.AppendLine("            <div class=\"info-row\">");
            html.AppendLine("                <div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Name:</label>");
            html.AppendLine($"                        <span>{invoice.StudentName}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Registration No.:</label>");
            html.AppendLine($"                        <span>{invoice.StudentRegNumber}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Class:</label>");
            html.AppendLine($"                        <span>{invoice.StudentClass}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Term:</label>");
            html.AppendLine($"                        <span>{invoice.Term}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Session/Year:</label>");
            html.AppendLine($"                        <span>{invoice.Session}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Payment Date:</label>");
            html.AppendLine($"                        <span>{invoice.PaymentDate:dd MMMM yyyy}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");

            // Payment Summary
            html.AppendLine("        <div class=\"invoice-section\">");
            html.AppendLine("            <h3>📋 Payment Summary</h3>");
            html.AppendLine("            <div class=\"payment-summary\">");
            html.AppendLine("                <div class=\"summary-item label\">");
            html.AppendLine("                    <span>Fee Structure & Payments</span>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"summary-item\">");
            html.AppendLine("                    <span>Total School Fees:</span>");
            html.AppendLine($"                    <span>₦{invoice.TotalFees:N2}</span>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"summary-item\">");
            html.AppendLine("                    <span>Previous Payments:</span>");
            html.AppendLine($"                    <span>₦{invoice.PreviousPayments:N2}</span>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"summary-item\">");
            html.AppendLine("                    <span>Previous Balance:</span>");
            html.AppendLine($"                    <span>₦{invoice.PreviousBalance:N2}</span>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"summary-item\" style=\"border-top: 2px dashed #dee2e6; padding-top: 8px; margin-top: 8px;\">");
            html.AppendLine("                    <span>Current Payment:</span>");
            html.AppendLine($"                    <span style=\"font-weight: 600; color: #0d6efd;\">₦{invoice.CurrentPayment:N2}</span>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class=\"summary-item total\">");
            html.AppendLine("                    <span>Remaining Balance:</span>");
            html.AppendLine($"                    <span>₦{invoice.NewBalance:N2}</span>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");

            // Payment Status
            html.AppendLine("        <div class=\"invoice-section\">");
            html.AppendLine("            <h3>ℹ️ Payment Status</h3>");
            html.AppendLine("            <div class=\"info-row\">");
            html.AppendLine("                <div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Payment Status:</label>");
            html.AppendLine($"                        <span><span class=\"status-badge {invoice.PaymentStatus.ToLower()}\">{invoice.PaymentStatus}</span></span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div>");
            html.AppendLine("                    <div class=\"info-item\">");
            html.AppendLine("                        <label>Payment State:</label>");
            html.AppendLine($"                        <span><span class=\"payment-state-badge\">{invoice.PaymentState}</span></span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");

            // Notes
            if (!string.IsNullOrEmpty(invoice.Narration) && invoice.Narration != "No additional notes")
            {
                html.AppendLine("        <div class=\"invoice-section\">");
                html.AppendLine("            <h3>💬 Notes & Comments</h3>");
                html.AppendLine("            <div class=\"alert alert-info\">");
                html.AppendLine($"                {invoice.Narration}");
                html.AppendLine("            </div>");
                html.AppendLine("        </div>");
            }

            // Evidence
            if (!string.IsNullOrEmpty(invoice.EvidenceFileName))
            {
                html.AppendLine("        <div class=\"invoice-section\">");
                html.AppendLine("            <h3>📄 Payment Evidence</h3>");
                html.AppendLine("            <div class=\"info-item\">");
                html.AppendLine("                <label>Evidence File:</label>");
                html.AppendLine($"                <span>{invoice.EvidenceFileName}</span>");
                html.AppendLine("            </div>");
                html.AppendLine("        </div>");
            }

            // Footer
            html.AppendLine("        <div class=\"invoice-footer\">");
            html.AppendLine("            <p><strong>This is an electronically generated invoice. No signature is required.</strong><br>");
            html.AppendLine($"            Generated on: {DateTime.Now:dd MMMM yyyy HH:mm:ss}<br>");
            html.AppendLine("            Invoice Valid Period: 30 days from issue date</p>");
            html.AppendLine("        </div>");

            html.AppendLine("    </div>");
            html.AppendLine("    <script>");
            html.AppendLine("        // Auto-print on page load (optional)");
            html.AppendLine("        // window.print();");
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Get CSS styles for standalone invoice
        /// </summary>
        private string GetInvoiceCss()
        {
            return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f5f5;
            padding: 20px;
            color: #333;
        }

        .invoice-container {
            max-width: 900px;
            margin: 0 auto;
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .invoice-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 3px solid #0d6efd;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }

        .company-info h2 {
            margin: 0 0 10px 0;
            color: #0d6efd;
            font-size: 24px;
        }

        .company-info p {
            margin: 5px 0;
            color: #666;
            font-size: 14px;
        }

        .invoice-info {
            text-align: right;
        }

        .invoice-info p {
            margin: 5px 0;
            font-size: 14px;
            color: #333;
        }

        .invoice-info-label {
            font-weight: 600;
            color: #0d6efd;
        }

        .invoice-section {
            margin-bottom: 30px;
        }

        .invoice-section h3 {
            border-left: 4px solid #0d6efd;
            padding-left: 15px;
            margin-bottom: 15px;
            color: #333;
            font-size: 16px;
        }
        .mt-2{
            margin-top: 15px;
        }
        .info-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 30px;
            margin-bottom: 15px;
        }

        .info-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
        }

        .info-item label {
            font-weight: 600;
            color: #555;
            min-width: 150px;
        }

        .info-item span {
            color: #333;
        }

        .payment-summary {
            background: #f8f9fa;
            border-left: 4px solid #0d6efd;
            padding: 20px;
            border-radius: 4px;
            margin-bottom: 30px;
        }

        .summary-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 12px;
            font-size: 15px;
        }

        .summary-item.total {
            border-top: 2px solid #dee2e6;
            padding-top: 12px;
            font-weight: 600;
            font-size: 16px;
            color: #0d6efd;
        }

        .summary-item.label {
            font-weight: 600;
            color: #555;
            margin-bottom: 15px;
        }

        .status-badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 13px;
            font-weight: 600;
        }

        .status-badge.pending {
            background-color: #fff3cd;
            color: #856404;
        }

        .status-badge.approved {
            background-color: #d4edda;
            color: #155724;
        }

        .status-badge.rejected {
            background-color: #f8d7da;
            color: #721c24;
        }

        .payment-state-badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 13px;
            font-weight: 600;
            background-color: #cfe2ff;
            color: #084298;
        }

        .alert {
            padding: 12px;
            border-radius: 4px;
            margin-bottom: 15px;
        }

        .alert-info {
            background-color: #d1ecf1;
            border: 1px solid #bee5eb;
            color: #0c5460;
        }

        .invoice-footer {
            text-align: center;
            padding-top: 30px;
            border-top: 2px solid #dee2e6;
            margin-top: 30px;
            color: #666;
            font-size: 13px;
        }

        .invoice-number-display {
            font-size: 18px;
            font-weight: 600;
            color: #0d6efd;
            font-family: 'Courier New', monospace;
        }

        .payment-reference {
            font-family: 'Courier New', monospace;
            background-color: #f0f0f0;
            padding: 8px 12px;
            border-radius: 4px;
            font-weight: 600;
        }

        @media print {
            body {
                background-color: white;
                padding: 0;
            }

            .invoice-container {
                box-shadow: none;
                padding: 0;
                max-width: 100%;
            }
        }

        @media (max-width: 768px) {
            .invoice-header {
                flex-direction: column;
            }

            .invoice-info {
                text-align: left;
                margin-top: 20px;
            }

            .info-row {
                grid-template-columns: 1fr;
                gap: 15px;
            }
        }
            ";
        }

        /// <summary>
        /// Get the standalone HTML invoice content
        /// Useful for emailing or further processing
        /// </summary>
        public async Task<string> GetHtmlInvoiceContentAsync(int paymentId)
        {
            try
            {
                var payment = await _unitOfWork.FeesPaymentServices.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return null;

                var invoice = await BuildInvoiceViewModelAsync(payment);
                return GenerateStandaloneHtmlInvoice(invoice);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generate a unique invoice number based on payment ID
        /// </summary>
        private string GenerateInvoiceNumber(int paymentId)
        {
            return $"INV-{DateTime.Now.Year}-{paymentId:D6}";
        }

        /// <summary>
        /// Generate a payment reference for tracking
        /// </summary>
        private string GeneratePaymentReference(int paymentId, int studentId)
        {
            return $"PAY-{DateTime.Now:yyyyMMdd}-{studentId:D4}-{paymentId:D4}";
        }
    }
}
