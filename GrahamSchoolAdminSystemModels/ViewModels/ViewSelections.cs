using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class ViewSelections
    {
        public IEnumerable<SelectListItem> SchoolClasses { get; set; }
        public IEnumerable<SelectListItem> AcademicSession { get; set; }
        public IEnumerable<SelectListItem> Terms { get; set; }
        public IEnumerable<SelectListItem> SubClass { get; set; }
        public IEnumerable<SelectListItem> PaymentItems { get; set; }
    }
}
