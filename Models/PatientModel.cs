using System;

namespace ProjectMaui.Models
{
    public class PatientModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
    }
}