using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PaymentDetailViewModel
    {
        // Payment Details
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string InvoiceNumber { get; set; }
        public string PaymentReference { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        // Student Information
        public string StudentName { get; set; }
        public string StudentRegNumber { get; set; }
        public string StudentClass { get; set; }
        public string ClassName { get; set; }

        // Academic Information
        public string Term { get; set; }
        public string Session { get; set; }
        public string SessionName { get; set; }

        // Amount Details
        public decimal TotalFees { get; set; }
        public decimal TotalPaidBefore { get; set; }
        public decimal PreviousPayments { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentPayment { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Amount { get; set; } // Used by Fees Payment module (same as CurrentPayment)
        public decimal NewBalance { get; set; }

        // Fee/Payment Item
        public string PaymentItemName { get; set; }
        public decimal ItemAmount { get; set; }

        // Payment Method
        public string PaymentChannel { get; set; }

        // Status Information
        public PaymentStatus? ApprovalStatus { get; set; }
        public string PaymentStatus { get; set; } // Pending, Approved, Rejected
        public string Status { get; set; } // Pending, Approved, Rejected
        public PaymentState PaymentState { get; set; } // Part Payment, Completed, Cancel
        public string PaymentStateString { get; set; }

        // Evidence/File
        public string FilePath { get; set; }
        public string EvidenceFileName { get; set; }
        public string EvidenceFilePath { get; set; }

        // Additional Information
        public string Message { get; set; }
        public string Notes { get; set; }
        public string Narration { get; set; }

        // Staff/Approval Info
        public string StaffUserId { get; set; }
        public string StaffName { get; set; }
        public string StaffEmail { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanCancel { get; set; }

        // School Information
        public string School { get; set; }
        public string SchoolContactInfo { get; set; }

        // Date for display
        public DateTime PaymentDate { get; set; }
    }
}
