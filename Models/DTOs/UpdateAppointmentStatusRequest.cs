namespace ProjectMaui.Models.DTOs
{
    public class UpdateAppointmentStatusRequest
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}