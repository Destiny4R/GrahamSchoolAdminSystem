using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PositionViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Position name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Position name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string Description { get; set; }
    }
}
