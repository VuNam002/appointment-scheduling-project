using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectMaui.Models
{
    public class AccountModel
    {
        public int AccountId { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public PatientModel Patient { get; set; }
        public DoctorInfoModel DoctorInfoModel { get; set; }
        public Roles Role { get; set; }
    }
}
