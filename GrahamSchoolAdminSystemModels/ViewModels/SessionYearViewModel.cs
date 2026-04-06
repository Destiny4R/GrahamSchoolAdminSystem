using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class SessionYearViewModel
    {
        public int? Id { get; set; }
        [Required, MaxLength(15)]
        public string Name { get; set; }
    }
}
