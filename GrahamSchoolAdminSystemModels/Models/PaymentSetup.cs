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
    public class PaymentSetup
    {
        public int Id { get; set; }
        public int PaymentItemId { get; set; }
        public int SessionId { get; set; }
        public Term Term { get; set; }
        public int ClassId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(PaymentItemId))]
        public PaymentItem PaymentItem { get; set; }
        [ForeignKey(nameof(SessionId))]
        public SessionYear Session { get; set; }
        [ForeignKey(nameof(ClassId))]
        public SchoolClasses SchoolClass { get; set; }
    }
}
