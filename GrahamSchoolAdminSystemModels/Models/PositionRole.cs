using System.ComponentModel.DataAnnotations.Schema;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class PositionRole
    {
        public int PositionId { get; set; }
        public string RoleId { get; set; }

        [ForeignKey(nameof(PositionId))]
        public PositionTable Position { get; set; }

        [ForeignKey(nameof(RoleId))]
        public ApplicationRole Role { get; set; }
    }
}
