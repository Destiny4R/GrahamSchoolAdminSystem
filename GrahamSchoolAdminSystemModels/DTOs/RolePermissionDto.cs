using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.DTOs
{
    public class RolePermissionDto
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
