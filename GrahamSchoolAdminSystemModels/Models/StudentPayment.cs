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
    public class StudentPayment
    {
        public int Id { get; set; }

        [ForeignKey(nameof(TermRegId))]
        public int TermRegId { get; set; }
        public TermRegistration TermRegistration { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public string Reference { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public PaymentState State { get; set; }  = PaymentState.Pending;
        [StringLength(120)]
        public string? Narration { get; set; }

        [StringLength(120)]
        public string? RejectMessage { get; set; }
        [StringLength(420)]
        public string? EvidenceFilePath { get; set; }

        public ICollection<StudentPaymentItem> PaymentItems { get; set; }
    }
}
