// Models/AppointmentModel.cs
using System;

namespace ProjectMaui.Models
{
    public class AppointmentModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } 
        public string Reason { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public DoctorInfoModel Doctor { get; set; }
        public PatientModel Patient { get; set; }

        public string DoctorName => Doctor?.DoctorName ?? "";
        public string PatientName => Patient?.PatientName ?? "";
        public string AppointmentDateDisplay => AppointmentDate.ToString("dd/MM/yyyy HH:mm");
        public string AppointmentTimeDisplay => AppointmentDate.ToString("HH:mm");
        public string AppointmentDayDisplay => AppointmentDate.ToString("dd/MM/yyyy");

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