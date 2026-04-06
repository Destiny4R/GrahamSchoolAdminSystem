using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PaymentInvoiceViewModel
    {
        // Invoice Details
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string PaymentReference { get; set; }

        // Student Information
        public string StudentName { get; set; }
        public string StudentRegNumber { get; set; }
        public string StudentClass { get; set; }
        public string ClassName { get; set; }

        // Academic Information
        public string Term { get; set; }
        public string Session { get; set; }
        public string SessionName { get; set; }

        // Payment Details
        public decimal TotalFees { get; set; }
        public decimal PreviousPayments { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentPayment { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal NewBalance { get; set; }
        public string PaymentItemName { get; set; }
        public string PaymentChannel { get; set; }

        // Payment Status
        public string PaymentStatus { get; set; } // Pending, Approved, Rejected
        public string Status { get; set; } // Pending, Approved, Rejected
        public string PaymentState { get; set; } // Part Payment, Completed, Cancel
        public DateTime PaymentDate { get; set; }

        // Evidence
        public string EvidenceFileName { get; set; }
        public string EvidenceFilePath { get; set; }

        // Additional Information
        public string Narration { get; set; }
        public string Notes { get; set; }
        public string School { get; set; }
        public string SchoolContactInfo { get; set; }
    }
}
