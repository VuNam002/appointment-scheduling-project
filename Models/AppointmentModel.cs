using System;

namespace ProjectMaui.Models
{
    public class AppointmentModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Notes { get; internal set; }
    }
}