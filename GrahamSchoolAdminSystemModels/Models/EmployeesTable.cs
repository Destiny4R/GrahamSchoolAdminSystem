using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class EmployeesTable
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string FullName { get; set; }
        [StringLength(11)]
        public string Phone { get; set; }
        public Gender Gender { get; set; }
        [StringLength(150)]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }

        // Navigation: many-to-many positions
        public ICollection<EmployeePosition> EmployeePositions { get; set; }
    }
}
