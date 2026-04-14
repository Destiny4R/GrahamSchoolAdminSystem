using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public bool FeesPartPayment { get; set; } = false;
        public bool PTAPartPayment { get; set; } = false;
        public bool PaymentEvidence { get; set; } = true;
        public Term Term { get; set; }
        public int SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public SessionYear SessionYear { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
