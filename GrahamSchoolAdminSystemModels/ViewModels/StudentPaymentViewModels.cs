using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    /// <summary>
    /// Request model for looking up a student's payable items
    /// </summary>
    public class PaymentLookupViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string AdmissionNo { get; set; }
    }

    /// <summary>
    /// Represents a single payable item within a category group
    /// </summary>
    public class PayableItemViewModel
    {
        public int PaymentItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal AlreadyPaid { get; set; }
        public decimal Remaining => ExpectedAmount - AlreadyPaid;
        public bool IsFullyPaid => Remaining <= 0;
    }

    /// <summary>
    /// Groups payable items by category for UI accordion display
    /// </summary>
    public class CategoryGroupViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<PayableItemViewModel> Items { get; set; } = new();
    }

    /// <summary>
    /// Full data needed for the make-payment page
    /// </summary>
    public class MakePaymentPageViewModel
    {
        public int TermRegistrationId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public List<CategoryGroupViewModel> CategoryGroups { get; set; } = new();
    }

    /// <summary>
    /// Line item submitted from the payment form
    /// </summary>
    public class StudentPaymentItemVM
    {
        public int PaymentItemId { get; set; }
        public decimal AmountPaid { get; set; }
    }

    /// <summary>
    /// Payload submitted when creating a payment
    /// </summary>
    public class CreatePaymentViewModel
    {
        [Required]
        public int TermRegistrationId { get; set; }

        [StringLength(120)]
        public string? Narration { get; set; }

        [Required]
        public List<StudentPaymentItemVM> Items { get; set; } = new();
    }

    /// <summary>
    /// Receipt display after payment
    /// </summary>
    public class PaymentReceiptViewModel
    {
        public int PaymentId { get; set; }
        public int TermRegId { get; set; }
        public string Reference { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string State { get; set; }
        public string? Narration { get; set; }
        public string? RejectMessage { get; set; }
        public string? EvidenceFilePath { get; set; }
        public List<ReceiptLineItem> LineItems { get; set; } = new();
    }

    public class ReceiptLineItem
    {
        public string CategoryName { get; set; }
        public string ItemName { get; set; }
        public decimal AmountPaid { get; set; }
    }

    /// <summary>
    /// Consolidated receipt showing all payments for a term registration grouped by category
    /// </summary>
    public class ConsolidatedReceiptViewModel
    {
        public int TermRegistrationId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }
        public string TermName { get; set; }
        public DateTime PrintDate { get; set; } = DateTime.UtcNow;
        public List<ReceiptCategoryGroup> Categories { get; set; } = new();
        public decimal GrandTotal => Categories.Sum(c => c.CategoryTotal);
    }

    /// <summary>
    /// Groups receipt line items under a single payment category with a subtotal
    /// </summary>
    public class ReceiptCategoryGroup
    {
        public string CategoryName { get; set; }
        public List<string> PaymentReferences { get; set; } = new();
        public List<ReceiptLineItem> Items { get; set; } = new();
        public decimal CategoryTotal => Items.Sum(i => i.AmountPaid);
    }

    /// <summary>
    /// Request model for updating payment state (approve/reject/cancel)
    /// </summary>
    public class UpdatePaymentStateRequest
    {
        [Required]
        public int PaymentState { get; set; }

        [StringLength(120)]
        public string? RejectMessage { get; set; }
    }

    /// <summary>
    /// Notification item for pending payment approvals
    /// </summary>
    public class PendingPaymentNotification
    {
        public int PaymentId { get; set; }
        public string Reference { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TimeAgo { get; set; }
    }
}
