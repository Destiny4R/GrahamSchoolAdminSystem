using static GrahamSchoolAdminSystemModels.Models.GetEnums;
using Microsoft.AspNetCore.Http;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class RecordPTAPaymentViewModel
    {
        public int termregid { get; set; }
        public int PtaFeesSetUpid { get; set; }
        public decimal amount { get; set; }
        public IFormFile evidencefile { get; set; }
        public string? narration { get; set; }
        public string? regnumber { get; set; }
        public string session { get; set; }
        public Term term { get; set; }
        public string name { get; set; }
        public string schoolclass { get; set; }
        public decimal ptafees { get; set; }
        public decimal balance { get; set; }
    }
}
