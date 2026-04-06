namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class EmployeePositionViewModel
    {
        public int EmployeeId { get; set; }

        public int PositionId { get; set; }

        public string EmployeeName { get; set; }

        public string PositionName { get; set; }

        // List of current positions for display in modal
        public List<string> CurrentPositions { get; set; } = new();
    }
}
