namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PTAFeesReportLineItemViewModel
    {
        public string Session { get; set; }
        public string TermName { get; set; }
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public decimal FeeAmountPerStudent { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal CollectionPercentage { get; set; }
    }
}
