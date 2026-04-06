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
    public class TermlyFeesSetup
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public Term Term { get; set; }
        public int SchoolClassId { get; set; }
        public int SessionId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(SessionId))]
        public SessionYear SessionYear { get; set; }
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses SchoolClass { get; set; }
    }
}
