namespace GrahamSchoolAdminSystemModels.DTOs
{
    public class StudentDto
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string surname { get; set; }
        public string firstname { get; set; }
        public string othername { get; set; }
        public string email { get; set; }
        public int genderId { get; set; }
        public string gender { get; set; }
        public bool isActive { get; set; }
        public DateTime createdDate { get; set; }
    }
}
