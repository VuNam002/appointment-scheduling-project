using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ProjectMaui.Models;
using ProjectMaui.Services;

namespace ProjectMaui.ViewModels
{
    public class BookingViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly DoctorInfoModel _selectedDoctor;

        public DoctorInfoModel SelectedDoctor => _selectedDoctor;
        public ObservableCollection<DoctorScheduleModel> DoctorSchedules { get; set; }

        private string _patientName;
        public string PatientName
        {
            get => _patientName;
            set
            {
                _patientName = value;
                OnPropertyChanged();
            }
        }

        private string _patientPhone;
        public string PatientPhone
        {
            get => _patientPhone;
            set
            {
                _patientPhone = value;
                OnPropertyChanged();
            }
        }

        private string _patientAddress;
        public string PatientAddress
        {
            get => _patientAddress;
            set
            {
                _patientAddress = value;
                OnPropertyChanged();
            }
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                _ = LoadAvailableTimesAsync();
            }
        }

        private TimeSpan _selectedTime = new TimeSpan(9, 0, 0);
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
            }
        }

        private string _reason;
        public string Reason
        {
            get => _reason;
            set
            {
                _reason = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _scheduleInfo;
        public string ScheduleInfo
        {
            get => _scheduleInfo;
            set
            {
                _scheduleInfo = value;
                OnPropertyChanged();
            }
        }

        public ICommand BookAppointmentCommand { get; }
        public ICommand LoadScheduleCommand { get; }

        public BookingViewModel(DoctorInfoModel doctor)
        {
            _selectedDoctor = doctor;
            _databaseService = new DatabaseService();
            DoctorSchedules = new ObservableCollection<DoctorScheduleModel>();

            BookAppointmentCommand = new Command(async () => await BookAppointmentAsync());
            LoadScheduleCommand = new Command(async () => await LoadDoctorScheduleAsync());

            _ = LoadDoctorScheduleAsync();
        }

        private async Task LoadDoctorScheduleAsync()
        {
            IsLoading = true;
            try
            {
                var schedules = await _databaseService.GetDoctorScheduleAsync(_selectedDoctor.DoctorId);
                DoctorSchedules.Clear();
                foreach (var schedule in schedules)
                {
                    DoctorSchedules.Add(schedule);
                }

                // Build schedule info string
                if (DoctorSchedules.Count > 0)
                {
                    var info = "Lịch làm việc:\n";
                    foreach (var schedule in DoctorSchedules)
                    {
                        info += $"• {schedule.DayOfWeekName}: {schedule.TimeDisplay}\n";
                    }
                    ScheduleInfo = info.TrimEnd('\n');
                }
                else
                {
                    ScheduleInfo = "Chưa có lịch làm việc";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading schedule: {ex.Message}");
                ScheduleInfo = "Không thể tải lịch làm việc";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAvailableTimesAsync()
        {
            // TODO: Load available time slots based on selected date
        }

        private async Task BookAppointmentAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập họ tên", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PatientPhone))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập số điện thoại", "OK");
                return;
            }

            if (PatientPhone.Length < 10)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Số điện thoại không hợp lệ", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                // Check or create patient
                var patient = await _databaseService.GetPatientByPhoneAsync(PatientPhone);
                if (patient == null)
                {
                    var newPatient = new PatientModel
                    {
                        PatientName = PatientName,
                        Phone = PatientPhone,
                        Address = PatientAddress
                    };
                    var patientId = await _databaseService.AddPatientAsync(newPatient);
                    if (patientId <= 0)
                    {
                        await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể tạo thông tin bệnh nhân", "OK");
                        return;
                    }
                    patient = newPatient;
                    patient.PatientId = patientId;
                }

                // Create appointment
                var appointmentDate = SelectedDate.Date + SelectedTime;
                var appointment = new AppointmentModel
                {
                    DoctorId = _selectedDoctor.DoctorId,
                    PatientId = patient.PatientId,
                    AppointmentDate = appointmentDate,
                    Reason = Reason,
                    Status = "Chờ xác nhận"
                };

                var appointmentId = await _databaseService.CreateAppointmentAsync(appointment);
                if (appointmentId > 0)
                {
                    await App.Current.MainPage.DisplayAlert(
                        "Thành công",
                        $"Đặt lịch hẹn thành công!\n\nBác sĩ: {_selectedDoctor.DoctorName}\nThời gian: {appointmentDate:dd/MM/yyyy HH:mm}",
                        "OK"
                    );

                    // Clear form
                    PatientName = "";
                    PatientPhone = "";
                    PatientAddress = "";
                    Reason = "";

                    // Navigate back
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể đặt lịch hẹn. Vui lòng chọn thời gian khác.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}