using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class SelectOptionsData
    {
        [Required]
        public int sessionid { get; set; }
        [Required]
        public int schoolclass { get; set; }
        [Required]
        public int subclass { get; set; }
        [Required]
        public int term { get; set; }
    }

    public class SelectOptionsDataForTermReg
    {
        public List<int> studentsid { get; set; }
        public SelectOptionsData data { get; set; }
    }
}
