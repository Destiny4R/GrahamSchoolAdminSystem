using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.Models
{
    public class StudentTable
    {
        public int Id { get; set; }
        [StringLength(70)]
        public string Surname { get; set; }
        [StringLength(70)]
        public string? Othername { get; set; }
        [StringLength(70)]
        public string Firstname { get; set; }
        public Gender Gender { get; set; }
        [StringLength(470)]
        public string PaspportPath { get; set; }
        [StringLength(470)]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<TermRegistration> TermRegistrations { get; set; } = new List<TermRegistration>();
        [NotMapped]
        public string FullName => $"{Surname} {Othername} {Firstname}";
    }
}
