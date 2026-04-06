using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class StudentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Firstname is required")]
        [StringLength(50, ErrorMessage = "Firstname cannot exceed 50 characters")]
        public string Firstname { get; set; }

        [StringLength(50, ErrorMessage = "Othername cannot exceed 50 characters")]
        public string? Othername { get; set; }

        [Required(ErrorMessage = "Student Reg. Number is required"), StringLength(50, ErrorMessage = "Student Reg. Number cannot exceed 50 characters"), Display(Name = "Student Reg. Number")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public int? GenderId { get; set; }

        public string? PassportPath { get; set; }

        public string? ApplicationUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        // Full name for display
        public string FullName => $"{Surname} {Firstname} {Othername}";

        // Gender display
        public string GenderDisplay => GenderId.HasValue ? ((Gender)GenderId.Value).ToString() : "Not Specified";
    }
}
