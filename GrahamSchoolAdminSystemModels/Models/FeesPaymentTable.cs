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
    public class FeesPaymentTable
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal Fees { get; set; }
        public int TermRegId { get; set; }
        public int TermlyFeesId { get; set; }
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
        public TermRegistration TermRegistration { get; set; }
        [ForeignKey(nameof(TermlyFeesId))]
        public TermlyFeesSetup TermlyFeesSetup { get; set; }
        //ApplicationUserId of the staff performing this transaction
        [StringLength(470)]
        public string StaffUserId { get; set; }
    }
}
