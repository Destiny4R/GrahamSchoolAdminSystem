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
    public class PTAFeesPayments
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; } = 0;
        public decimal Fees { get; set; }
        public int TermRegId { get; set; }
        public int PtaFeesSetupId { get; set; }
        [StringLength(450)]
        public string FilePath { get; set; }
        [StringLength(100)]
        public string InvoiceNumber { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentState PaymentState { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [StringLength(500)]
        public string? Message { get; set; }
        [StringLength(1000)]
        public string? Narration { get; set; }
        [ForeignKey(nameof(TermRegId))]
        public TermRegistration TermRegistration { get; set; }
        [ForeignKey(nameof(PtaFeesSetupId))]
        public PTAFeesSetup PTAFeesSetup { get; set; }
        //ApplicationUserId of the staff performing this transaction
        [StringLength(470)]
        public string StaffUserId { get; set; }
    }
}
