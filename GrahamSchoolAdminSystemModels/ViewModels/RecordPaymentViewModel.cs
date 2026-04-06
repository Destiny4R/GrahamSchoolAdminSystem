using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class RecordPaymentViewModel
    {
        [Required]
        public int termregid { get; set; }
        [Required]
        public int FeesSetUpid { get; set; }
        public string name { get; set; }
        public string session { get; set; }
        public string term { get; set; }
        public string schoolclass { get; set; }
        [Required]
        public decimal amount { get; set; }
        public decimal feespayment { get; set; }
        public decimal totalamount { get; set; }
        public decimal balance { get; set; }
        [StringLength(400)]
        public string narration { get; set; }        
        public string regnumber { get; set; }
        [Required]
        public IFormFile evidenncefile { get; set; }
        public string EvidenceFilePath { get; set; }
        public string? otherPayment { get; set; }
    }
}