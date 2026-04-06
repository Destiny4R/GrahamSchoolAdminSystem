using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class GetEnums
    {
        public enum Gender { Male = 1, Female = 2 }
        public enum Term { First = 1, Second = 2, Third = 3 }
        public enum PaymentStatus { Pending = 1, Approved = 2, Rejected = 3 }
        public enum PaymentState {
            [Display(Name = "Part Payment")]
            PartPayment = 1,
            Completed = 2,
            Cancelled = 3
        }
    }

}
