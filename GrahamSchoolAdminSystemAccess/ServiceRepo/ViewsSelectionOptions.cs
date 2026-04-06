using GrahamSchoolAdminSystemAccess.Data;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess.ServiceRepo
{
    public class ViewsSelectionOptions : IViewsSelectionOptions
    {
        private readonly ApplicationDbContext _db;

        public ViewsSelectionOptions(ApplicationDbContext db)
        {
            this._db = db;
        }
        public async Task<IEnumerable<SelectListItem>> GetSchoolClassesForDropdownAsync()
        {
            var classes = await _db.SchoolClasses.OrderBy(k=>k.Name).ToListAsync();
            return classes.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });
        }

        public async Task<IEnumerable<SelectListItem>> GetSchoolSubclassesForDropdownAsync()
        {
            var subclasses = await _db.SchoolSubClasses.OrderBy(k => k.Name).ToListAsync();
            return subclasses.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
        }

        public async Task<IEnumerable<SelectListItem>> GetSessionsForDropdownAsync()
        {
            var sessions = await _db.SessionYears.OrderByDescending(s => s.CreatedDate).ToListAsync();
            return sessions.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            });
        }

    }
}
