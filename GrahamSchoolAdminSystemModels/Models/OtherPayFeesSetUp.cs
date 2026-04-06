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
    public class OtherPayFeesSetUp
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int SchoolClassId { get; set; }
        public Term Term { get; set; }
        public double Amount { get; set; }
        public int OtherPayId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClasses Schoolclasses { get; set; }
        [ForeignKey(nameof(OtherPayId))]
        public OtherPayItemsTable OtherPayItems { get; set; }
        [ForeignKey(nameof(SessionId))]
        public SessionYear SessionYear { get; set; }
    }
}
