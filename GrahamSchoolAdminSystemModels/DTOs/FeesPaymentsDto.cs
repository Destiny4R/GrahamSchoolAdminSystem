using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.DTOs
{
    public class FeesPaymentsDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string term { get; set; }
        public string session { get; set; }
        public string fees { get; set; }
        public string amount { get; set; }
        public string schoolclass { get; set; }
        public string balance { get; set; }
        public string regnumber { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public string paymentitem { get; set; }
        public int? paymentitemid { get; set; }
        public DateTime createdate { get; set; }
    }
}
