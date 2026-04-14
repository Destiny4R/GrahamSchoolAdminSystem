using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class PaymentCategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(300)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PaymentItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Item Name")]
        public string Name { get; set; }

        [StringLength(300)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Display helper
        public string? CategoryName { get; set; }
    }

    public class PaymentSetupViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Payment Item")]
        public int PaymentItemId { get; set; }

        [Required]
        [Display(Name = "Academic Session")]
        public int SessionId { get; set; }

        [Required]
        [Display(Name = "Term")]
        public Term Term { get; set; }

        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;

        // Display helpers
        public int? CategoryId { get; set; }
        public string? PaymentItemName { get; set; }
        public string? SessionName { get; set; }
        public string? TermName { get; set; }
        public string? ClassName { get; set; }
        public string? CategoryName { get; set; }
    }
}
