namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class FeesReportViewModel
    {
        public List<FeesReportLineItemViewModel> ReportData { get; set; } = new();
        
        // Filter properties
        public int? SelectedSessionId { get; set; }
        public int? SelectedTerm { get; set; }
        public int? SelectedClassId { get; set; }

        // Dropdown lists for filters
        public List<SessionDropdownViewModel> Sessions { get; set; } = new();
        public List<TermDropdownViewModel> Terms { get; set; } = new();
        public List<ClassDropdownViewModel> Classes { get; set; } = new();

        // Summary totals
        public int TotalStudents { get; set; }
        public decimal TotalExpected { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OverallCollectionPercentage => TotalExpected > 0 
            ? Math.Round((TotalActual / TotalExpected) * 100, 2) 
            : 0;
    }

    public class SessionDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TermDropdownViewModel
    {
        public int Value { get; set; }
        public string Name { get; set; }
    }

    public class ClassDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
