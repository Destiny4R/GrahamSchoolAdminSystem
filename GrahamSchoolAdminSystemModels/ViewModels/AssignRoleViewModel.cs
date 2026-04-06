using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class AssignRoleViewModel
    {
        [Required]
        public int PositionId { get; set; }

        public string PositionName { get; set; }

        [Required(ErrorMessage = "At least one role must be selected")]
        public List<string> SelectedRoleIds { get; set; } = new List<string>();

        public List<RoleCheckboxViewModel> AvailableRoles { get; set; } = new List<RoleCheckboxViewModel>();

        public List<string> AssignedRoles { get; set; } = new List<string>();
    }

    public class RoleCheckboxViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsAssigned { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
