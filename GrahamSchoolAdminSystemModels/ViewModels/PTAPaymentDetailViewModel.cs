using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PTAPaymentDetailViewModel
    {
        public int PaymentId { get; set; }
        public string InvoiceNumber { get; set; }
        public string StudentName { get; set; }
        public string StudentRegNumber { get; set; }
        public string AcademicSession { get; set; }
        public string Term { get; set; }
        public string SchoolClass { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalPTAFees { get; set; }
        public decimal TotalPaidBefore { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentState PaymentState { get; set; }
        public string FilePath { get; set; }
        public string Message { get; set; }
        public string Narration { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string StaffUserId { get; set; }
        public string StaffName { get; set; }
        public string StaffEmail { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanCancel { get; set; }
    }
}
