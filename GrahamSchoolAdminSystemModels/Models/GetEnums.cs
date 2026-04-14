namespace GrahamSchoolAdminSystemModels.Models
{
    public class GetEnums
    {
        public enum Gender { Male = 1, Female = 2 }
        public enum Term { First = 1, Second = 2, Third = 3 }
        public enum PaymentStatus
        {
            Pending = 1,
            Completed = 2,
            Failed = 3,
            Reversed = 4
        }

        public enum PaymentState
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3,
            Cancelled = 4
        }
    }
}
