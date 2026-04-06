using System.ComponentModel.DataAnnotations;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Phone must contain at least 10 digits")]
        public string Phone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string Department { get; set; }

        [StringLength(150, ErrorMessage = "Address cannot exceed 150 characters")]
        public string Address { get; set; }

        public int? GenderId { get; set; }

        public string ApplicationUserId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property for display
        public List<string> Positions { get; set; } = new();

        // Full name for display
        public string FullName => $"{FirstName} {LastName}";
    }
}
