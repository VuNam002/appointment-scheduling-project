using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProjectMaui.Services;

namespace ProjectMaui.Models
{
    public class AppointmentDetailModel : INotifyPropertyChanged
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string AppointmentDateDisplay => AppointmentDate.ToString("dd/MM/yyyy HH:mm");

        private string _reason;
        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusColor)); // Báo cho UI biết StatusColor cũng thay đổi
                    OnPropertyChanged(nameof(CanCancel));
                    OnPropertyChanged(nameof(CanUpdateStatus));
                }
            }
        }

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Chờ xác nhận" => "#F39C12",
                    "Đã xác nhận" => "#3498DB",
                    "Hoàn thành" => "#27AE60",
                    "Đã hoàn thành" => "#27AE60", // Xanh lá
                    "Đã hủy" => "#E74C3C",
                    _ => "#95A5A6"
                };
            }
        }

        public bool CanCancel => UserSession.Current.Role == "Patient" && (Status == "Chờ xác nhận" || Status == "Đã xác nhận");
        public bool CanUpdateStatus => (UserSession.Current.Role == "Doctor" || UserSession.Current.Role == "Admin") && (Status == "Chờ xác nhận" || Status == "Đã xác nhận");


        public string Notes { get; set; }
        public string DoctorPhone { get; set; }
        public string DoctorEmail { get; set; }
        public string Specialization { get; set; }
        public string DoctorImage { get; set; }
        public string PatientPhone { get; set; }
        public string PatientAddress { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentLocation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}