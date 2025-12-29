namespace ProjectMaui.Models
{
    public class DoctorInfoModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? DepartmentId { get; set; }
        public string Specialization { get; set; }
        public string Image { get; set; }

        public DepartmentModel Department { get; set; }

        // Thuộc tính tiện ích để binding trực tiếp
        public string DepartmentName => Department?.DepartmentName;
    }
}