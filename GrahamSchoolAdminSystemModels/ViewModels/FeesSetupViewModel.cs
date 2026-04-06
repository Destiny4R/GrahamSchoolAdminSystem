using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class FeesSetupViewModel
    {
        public int? Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public Term Term { get; set; }
        [Required, Display(Name ="Class")]
        public int SchoolClassId { get; set; }
        [Required, Display(Name = "Academic Session")]
        public int SessionId { get; set; }
    }
}
