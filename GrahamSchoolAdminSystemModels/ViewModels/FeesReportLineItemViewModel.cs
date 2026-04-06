namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class FeesReportLineItemViewModel
    {
        public int SessionId { get; set; }
        public string Session { get; set; }

        public int Term { get; set; }
        public string TermName { get; set; }

        public int SchoolClassId { get; set; }
        public string ClassName { get; set; }

        public int StudentCount { get; set; }
        public decimal FeeAmountPerStudent { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        
        public decimal CollectionPercentage => ExpectedAmount > 0 
            ? Math.Round((ActualAmount / ExpectedAmount) * 100, 2) 
            : 0;
    }
}
