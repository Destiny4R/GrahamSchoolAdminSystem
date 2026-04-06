namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class DashboardViewModel
    {
        // ── General Counts ───────────────────────────────────────────────────
        public int TotalStudents { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveClasses { get; set; }

        // ── Termly School Fees ───────────────────────────────────────────────
        public decimal TermlyFeesExpected { get; set; }
        public decimal TermlyFeesCollected { get; set; }
        public decimal TermlyFeesOutstanding { get; set; }
        public int TermlyFeesApprovedCount { get; set; }
        public int TermlyFeesPendingCount { get; set; }
        public int TermlyFeesPartialCount { get; set; }
        public decimal TermlyFeesCollectionPct => TermlyFeesExpected > 0
            ? Math.Round(TermlyFeesCollected / TermlyFeesExpected * 100, 1) : 0;

        // ── PTA Fees ─────────────────────────────────────────────────────────
        public decimal PTAFeesExpected { get; set; }
        public decimal PTAFeesCollected { get; set; }
        public decimal PTAFeesOutstanding { get; set; }
        public int PTAFeesApprovedCount { get; set; }
        public int PTAFeesPendingCount { get; set; }
        public decimal PTAFeesCollectionPct => PTAFeesExpected > 0
            ? Math.Round(PTAFeesCollected / PTAFeesExpected * 100, 1) : 0;

        // ── Other Payments ───────────────────────────────────────────────────
        public decimal OtherPaymentsCollected { get; set; }
        public decimal OtherPaymentsPending { get; set; }
        public int OtherPaymentsApprovedCount { get; set; }
        public int OtherPaymentsPendingCount { get; set; }

        // ── Grand Totals ─────────────────────────────────────────────────────
        public decimal GrandTotalExpected => TermlyFeesExpected + PTAFeesExpected;
        public decimal GrandTotalCollected => TermlyFeesCollected + PTAFeesCollected + OtherPaymentsCollected;
        public decimal GrandTotalOutstanding => TermlyFeesOutstanding + PTAFeesOutstanding + OtherPaymentsPending;
        public decimal GrandCollectionPct => GrandTotalExpected > 0
            ? Math.Round(GrandTotalCollected / GrandTotalExpected * 100, 1) : 0;

        // ── Payment Approval Status (all types combined) ──────────────────────
        public int TotalApprovedPayments => TermlyFeesApprovedCount + PTAFeesApprovedCount + OtherPaymentsApprovedCount;
        public int TotalPendingPayments => TermlyFeesPendingCount + PTAFeesPendingCount + OtherPaymentsPendingCount;
        public int TotalRejectedPayments { get; set; }

        // ── Recent Payments ──────────────────────────────────────────────────
        public List<RecentPaymentItemViewModel> RecentPayments { get; set; } = new();

        // ── Current session label (for display) ──────────────────────────────
        public string CurrentSessionLabel { get; set; } = string.Empty;
    }

    public class RecentPaymentItemViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentState { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
