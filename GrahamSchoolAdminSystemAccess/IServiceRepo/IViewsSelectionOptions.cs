using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.IServiceRepo
{
    public interface IViewsSelectionOptions
    {
            Task<IEnumerable<SelectListItem>> GetSchoolClassesForDropdownAsync();
            Task<IEnumerable<SelectListItem>> GetSessionsForDropdownAsync();
            //Task<IEnumerable<SelectListItem>> GetTermsForDropdownAsync();
            Task<IEnumerable<SelectListItem>> GetSchoolSubclassesForDropdownAsync();
    }
}
