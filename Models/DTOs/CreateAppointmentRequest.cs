using System;

namespace ProjectMaui.Models.DTOs
{
    public class CreateAppointmentRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
    }
}