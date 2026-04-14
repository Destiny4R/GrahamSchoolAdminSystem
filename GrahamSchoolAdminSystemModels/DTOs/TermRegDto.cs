using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.DTOs
{
    public class TermRegDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public string term { get; set; }
        public string session { get; set; }
        public string schoolclass { get; set; }
        public string regnumber { get; set; }
        public DateTime createdate { get; set; }
        public bool hasPayment { get; set; }
    }
}
