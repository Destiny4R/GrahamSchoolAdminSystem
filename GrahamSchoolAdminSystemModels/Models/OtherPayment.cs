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
    public class OtherPayment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal ItemAmount { get; set; }
        public int TermRegId { get; set; }
        public int PayFeesSetUpId { get; set; }
        [StringLength(450)]
        public string FilePath { get; set; }
        [StringLength(500)]
        public string? Message { get; set; }
        [StringLength(500)]
        public string? Narration { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentState PaymentState { get; set; }
        [StringLength(100)]
        public string InvoiceNumber { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(TermRegId))]
        public TermRegistration Termregistration { get; set; }
        [ForeignKey(nameof(PayFeesSetUpId))]
        public OtherPayFeesSetUp OtherPayFeesSetUp { get; set; }
        [StringLength(450)]
        public string? StaffUserId { get; set; }

    }
}
