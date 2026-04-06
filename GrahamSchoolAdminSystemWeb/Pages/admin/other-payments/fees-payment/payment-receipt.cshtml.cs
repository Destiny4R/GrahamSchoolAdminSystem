using GrahamSchoolAdminSystemAccess.IServiceRepo;
using GrahamSchoolAdminSystemModels.Models;
using GrahamSchoolAdminSystemModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace GrahamSchoolAdminSystemWeb.Pages.admin.other_payments.fees_payment
{
    public class payment_receiptModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentInvoiceViewModel Invoice { get; set; } = new();

        public payment_receiptModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> OnGetAsync(int paymentId)
        {
            try
            {
                // Fetch payment record with all related data
                var payment = await _unitOfWork.OtherPaymentServices.GetOtherPaymentByIdAsync(paymentId);

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
                var payment = await _unitOfWork.OtherPaymentServices.GetOtherPaymentByIdAsync(paymentId);

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
                string fileName = $"Receipt_{invoice.InvoiceNumber.Replace("-", "_")}.html";

                return File(fileBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating receipt: {ex.Message}");
            }
        }

        /// <summary>
        /// Build invoice view model from payment data with installment history
        /// </summary>
        private async Task<PaymentInvoiceViewModel> BuildInvoiceViewModelAsync(OtherPayment payment)
        {
            // Get total amount for this payment item
            decimal totalFees = (decimal)payment.OtherPayFeesSetUp.Amount;

            // Get all approved payments for this payment item from the same student/term
            var approvedPayments = await _unitOfWork.OtherPaymentServices.GetApprovedPaymentsByItemAsync(
                payment.TermRegId, 
                payment.PayFeesSetUpId
            );

            // Calculate previous payments total (excluding current payment)
            decimal previousPaymentsTotal = approvedPayments
                .Where(p => p.Id != payment.Id)
                .Sum(p => p.Amount);

            // Calculate balance before this payment
            decimal previousBalance = totalFees - previousPaymentsTotal;

            // Calculate new balance after this payment
            decimal newBalance = previousBalance - payment.Amount;
            if (newBalance < 0) newBalance = 0; // Can't have negative balance

            var invoice = new PaymentInvoiceViewModel
            {
                InvoiceNumber = $"OFP-{payment.Id:D6}",
                InvoiceDate = payment.CreatedDate,
                StudentName = payment.Termregistration.Student.FullName,
                StudentRegNumber = payment.Termregistration.Student.ApplicationUser.UserName,
                ClassName = payment.Termregistration.SchoolClass.Name,
                SessionName = payment.Termregistration.SessionYear.Name,
                PaymentItemName = payment.OtherPayFeesSetUp.OtherPayItems.Name,
                AmountPaid = payment.Amount,
                TotalFees = totalFees,
                PreviousPayments = previousPaymentsTotal,
                PreviousBalance = previousBalance,
                CurrentPayment = payment.Amount,
                NewBalance = newBalance,
                PaymentReference = payment.InvoiceNumber,
                PaymentChannel = "Bank",
                Status = payment.Status.ToString() ?? "Pending",
                Notes = payment.Narration ?? "Payment recorded in system"
            };

            return invoice;
        }

        /// <summary>
        /// Generate standalone HTML invoice for download
        /// </summary>
        private string GenerateStandaloneHtmlInvoice(PaymentInvoiceViewModel invoice)
        {
            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Payment Receipt - ");
            sb.Append(invoice.InvoiceNumber);
            sb.Append(@"</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Arial', sans-serif; background: #f5f5f5; color: #333; }
        .container { max-width: 800px; margin: 40px auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #667eea; padding-bottom: 20px; }
        .header h1 { color: #667eea; font-size: 28px; margin-bottom: 5px; }
        .header p { color: #666; font-size: 14px; }
        .receipt-number { text-align: right; color: #999; font-size: 12px; margin-bottom: 20px; }
        .section { margin-bottom: 25px; }
        .section-title { font-size: 14px; font-weight: bold; color: #333; text-transform: uppercase; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px; }
        .row { display: flex; gap: 20px; margin-bottom: 10px; }
        .col { flex: 1; }
        .label { font-size: 12px; font-weight: bold; color: #666; text-transform: uppercase; margin-bottom: 3px; letter-spacing: 0.5px; }
        .value { font-size: 14px; color: #333; font-weight: 500; }
        .amount-box { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; }
        .amount-box .label { color: rgba(255,255,255,0.9); }
        .amount-box .amount { font-size: 32px; font-weight: bold; margin: 10px 0; }
        .badge { display: inline-block; padding: 5px 10px; border-radius: 4px; font-size: 11px; font-weight: bold; text-transform: uppercase; }
        .badge-pending { background: #ffc107; color: #000; }
        .badge-approved { background: #28a745; color: white; }
        .badge-rejected { background: #dc3545; color: white; }
        .footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; color: #999; font-size: 12px; }
        @media print { body { background: white; } .container { box-shadow: none; } }
    </style>
</head>
<body>
    <div class='container'>
        <div class='receipt-number'>Receipt #: ");
            sb.Append(invoice.InvoiceNumber);
            sb.Append("</div>\n        <div class='header'>\n            <h1>Payment Receipt</h1>\n            <p>Graham School Administration System</p>\n        </div>\n\n        <div class='section'>\n            <div class='section-title'>Student Information</div>\n            <div class='row'>\n                <div class='col'>\n                    <div class='label'>Full Name</div>\n                    <div class='value'>");
            sb.Append(invoice.StudentName);
            sb.Append("</div>\n                </div>\n                <div class='col'>\n                    <div class='label'>Registration Number</div>\n                    <div class='value'>");
            sb.Append(invoice.StudentRegNumber);
            sb.Append("</div>\n                </div>\n            </div>\n            <div class='row'>\n                <div class='col'>\n                    <div class='label'>Class</div>\n                    <div class='value'>");
            sb.Append(invoice.ClassName);
            sb.Append("</div>\n                </div>\n                <div class='col'>\n                    <div class='label'>Session</div>\n                    <div class='value'>");
            sb.Append(invoice.SessionName);
            sb.Append("</div>\n                </div>\n            </div>\n        </div>\n\n        <div class='section'>\n            <div class='section-title'>Payment Details</div>\n            <div class='row'>\n                <div class='col'>\n                    <div class='label'>Payment Item</div>\n                    <div class='value'>");
            sb.Append(invoice.PaymentItemName);
            sb.Append("</div>\n                </div>\n                <div class='col'>\n                    <div class='label'>Payment Reference</div>\n                    <div class='value'>");
            sb.Append(invoice.PaymentReference ?? "N/A");
            sb.Append("</div>\n                </div>\n            </div>\n            <div class='row'>\n                <div class='col'>\n                    <div class='label'>Payment Channel</div>\n                    <div class='value'>");
            sb.Append(invoice.PaymentChannel);
            sb.Append("</div>\n                </div>\n                <div class='col'>\n                    <div class='label'>Date</div>\n                    <div class='value'>");
            sb.Append(invoice.InvoiceDate.ToString("dd/MM/yyyy HH:mm"));
            sb.Append("</div>\n                </div>\n            </div>\n        </div>\n\n        <div class='amount-box'>\n            <div class='label'>Amount Paid</div>\n            <div class='amount'>₦");
            sb.Append(invoice.AmountPaid.ToString("N2"));
            sb.Append("</div>\n        </div>\n\n        <div class='section'>\n            <div class='section-title'>Payment Status</div>\n            <span class='badge badge-");
            sb.Append(invoice.Status?.ToLower().Replace(" ", "-") ?? "pending");
            sb.Append("'>");
            sb.Append(invoice.Status ?? "Pending");
            sb.Append("</span>\n        </div>\n\n        @if (!string.IsNullOrEmpty(invoice.Notes))\n        {\n            <div class='section'>\n                <div class='section-title'>Notes</div>\n                <div class='value'>");
            sb.Append(invoice.Notes);
            sb.Append("</div>\n            </div>\n        }\n\n        <div class='footer'>\n            <p>This is an automatically generated receipt. Please keep it for your records.</p>\n            <p>For inquiries, please contact the School Administration Office.</p>\n        </div>\n    </div>\n</body>\n</html>");

            return sb.ToString();
        }
    }
}
