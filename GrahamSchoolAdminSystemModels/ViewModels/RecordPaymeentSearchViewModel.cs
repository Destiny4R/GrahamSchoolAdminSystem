using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class RecordPaymeentSearchViewModel
    {
        [Required(ErrorMessage = "Student Registration Number is required."), StringLength(20, ErrorMessage = "Student Registration Number cannot exceed 20 characters.")]
        public string StudentRegNumber { get; set; }
        [Required]
        public int sessionid { get; set; }
        [Required]
        public int schoolclass { get; set; }
        [Required]
        public int term { get; set; }
        [Required]
        public int otherpayitemid { get; set; }
    }
}
