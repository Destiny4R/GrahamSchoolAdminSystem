using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class PositionTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation: many-to-many with Employees via EmployeePosition
        public ICollection<EmployeePosition> EmployeePositions { get; set; }

        // Navigation: many-to-many with Identity roles via PositionRole
        public ICollection<PositionRole> PositionRoles { get; set; }
    }
}
