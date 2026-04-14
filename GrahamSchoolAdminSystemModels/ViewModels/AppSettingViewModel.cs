using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class AppSettingViewModel
    {
        public int? id { get; set; }
        [Display(Name ="Fees Part Payment")]
        public bool feespart { get; set; }
        [Display(Name = "PTA Part Payment")]
        public bool ptapart { get; set; }
        [Display(Name = "PaymentEvidence")]
        public bool PaymentEvidence { get; set; }
        [Display(Name = "Current Term")]
        public int term { get; set; }
        [Display(Name = "Current Session")]
        public int sessionId { get; set; }
    }
}
