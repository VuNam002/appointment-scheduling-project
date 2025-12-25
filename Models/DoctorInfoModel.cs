// Models/DoctorInfoModel.cs
using System;

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

        public string DepartmentName => Department?.DepartmentName ?? "";
        public string DisplayInfo => $"{DoctorName} - {Specialization}";
    }
}