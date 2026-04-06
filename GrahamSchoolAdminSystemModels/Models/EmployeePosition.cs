using System.ComponentModel.DataAnnotations.Schema;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class EmployeePosition
    {
        public int EmployeeId { get; set; }
        public int PositionId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public EmployeesTable Employee { get; set; }

        [ForeignKey(nameof(PositionId))]
        public PositionTable Position { get; set; }
    }
}
