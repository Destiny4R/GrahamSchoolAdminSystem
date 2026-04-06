using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class SchoolClassViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Class Name")]
        public string Name { get; set; }
    }
}
