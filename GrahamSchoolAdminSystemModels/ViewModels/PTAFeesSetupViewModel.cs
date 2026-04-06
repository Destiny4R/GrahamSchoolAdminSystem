using System.ComponentModel.DataAnnotations;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PTAFeesSetupViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "School class is required")]
        [Display(Name = "School Class")]
        public int SchoolClassId { get; set; }

        [Required(ErrorMessage = "Academic session is required")]
        [Display(Name = "Academic Session")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Term is required")]
        [Display(Name = "Term")]
        public int Term { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}
