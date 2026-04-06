using GrahamSchoolAdminSystemModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.DTOs
{
    public class FeeSetupDto
    {
        public int id { get; set; }
        public string amount1 { get; set; }
        public decimal amount { get; set; }
        public Term Term { get; set; }
        public string term1 { get; set; }
        public int classid { get; set; }
        public int sessionid { get; set; }
        public DateTime createdate { get; set; }
        public string sessionname { get; set; }
        public string classname { get; set; }
    }
}
