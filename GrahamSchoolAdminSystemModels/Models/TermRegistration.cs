using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class TermRegistration
    {
        public int Id { get; set; }
        public int SchoolClassId { get; set; }
        public int SessionId { get; set; }
        public Term Term { get; set; }
        public int SchoolSubclassId { get; set; }
        public int StudentId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(SessionId))]
        public SessionYear SessionYear { get; set; }
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClass { get; set; }
        [ForeignKey(nameof(SchoolSubclassId))]
        public SchoolSubClass SchoolSubClass { get; set; }
        [ForeignKey(nameof(StudentId))]
        public StudentTable Student { get; set; }
    }
}
