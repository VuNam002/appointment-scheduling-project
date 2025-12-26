using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectMaui.Services
{
    public class UserSession
    {
        private static UserSession _instance;

        public static UserSession Current => _instance ??= new UserSession();

        private UserSession() { }

        public int AccountId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; } 
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }

        public bool IsLoggedIn => AccountId > 0;

        public void Clear()
        {
            AccountId = 0;
            PatientId = 0;
            DoctorId = 0; // Reset cả DoctorId
            PhoneNumber = null;
            FullName = null;
            Role = null;
        }
    }
}