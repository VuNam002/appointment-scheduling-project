using System;

namespace ProjectMaui.Models
{
    public class AppointmentDetailModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Chờ xác nhận" => "#F39C12", // Cam
                    "Đã xác nhận" => "#3498DB", // Xanh dương
                    "Hoàn thành" => "#27AE60", // Xanh lá
                    "Đã hủy" => "#E74C3C", // Đỏ
                    _ => "#95A5A6" // Xám
                };
            }
        }

        public string Notes { get; internal set; }
        public string DoctorPhone { get; internal set; }
        public string DoctorEmail { get; internal set; }
        public string Specialization { get; internal set; }
        public string DoctorImage { get; internal set; }
        public string PatientPhone { get; internal set; }
        public string PatientAddress { get; internal set; }
        public string DepartmentName { get; internal set; }
        public string DepartmentLocation { get; internal set; }
    }
}