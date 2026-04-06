using static GrahamSchoolAdminSystemModels.Models.GetEnums;

namespace GrahamSchoolAdminSystemModels.ViewModels
{
    public class RecordPTAPaymentSearchViewModel
    {
        public string StudentRegNumber { get; set; }
        public int term { get; set; }
        public int sessionid { get; set; }
        public int schoolclass { get; set; }
    }
}
