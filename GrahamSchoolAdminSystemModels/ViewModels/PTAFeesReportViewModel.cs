namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PTAFeesReportViewModel
    {
        public List<SessionDropdownViewModel> Sessions { get; set; } = new();
        public List<TermDropdownViewModel> Terms { get; set; } = new();
        public List<ClassDropdownViewModel> Classes { get; set; } = new();
        public List<PTAFeesReportLineItemViewModel> ReportData { get; set; } = new();
        public int TotalStudents { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OverallCollectionPercentage { get; set; }
        public int? SelectedSessionId { get; set; }
        public int? SelectedTerm { get; set; }
        public int? SelectedClassId { get; set; }
    }
}
