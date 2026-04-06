using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class OtherPayItemsViewModel
    {
        public int? Id { get; set; }
        [StringLength(100)]
        public string name { get; set; }
        [StringLength(250)]
        public string description { get; set; }
    }
}
