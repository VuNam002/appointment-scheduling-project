using System;

namespace ProjectMaui.Models
{
    public class AppointmentDetailModel
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }

        // Doctor info
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorPhone { get; set; }
        public string DoctorEmail { get; set; }
        public string Specialization { get; set; }
        public string DoctorImage { get; set; }

        // Patient info
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string PatientAddress { get; set; }

        // Department info
        public string DepartmentName { get; set; }
        public string DepartmentLocation { get; set; }

        // Display properties
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd/MM/yyyy HH:mm");
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Chờ xác nhận" => "#FFA500",
                    "Đã xác nhận" => "#4CAF50",
                    "Hoàn thành" => "#2196F3",
                    "Đã hủy" => "#F44336",
                    _ => "#757575"
                };
            }
        }
    }
}