using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ProjectMaui.Models;
using ProjectMaui.Services;

namespace ProjectMaui.ViewModels
{
    public class AppointmentListViewModel : INotifyPropertyChanged
    {
        private readonly AppointmentService _appointmentService;

        public ObservableCollection<AppointmentDetailModel> Appointments { get; set; }
        public ObservableCollection<AppointmentDetailModel> FilteredAppointments { get; set; }

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

        private string _selectedFilter = "Tất cả";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged();
                FilterAppointments();
            }
        }

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CancelAppointmentCommand { get; }

        public AppointmentListViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            Appointments = new ObservableCollection<AppointmentDetailModel>();
            FilteredAppointments = new ObservableCollection<AppointmentDetailModel>();

            LoadAppointmentsCommand = new Command(async () => await LoadAppointmentsAsync());
            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());
            CancelAppointmentCommand = new Command<int>(async (id) => await CancelAppointmentAsync(id));

            _ = LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            IsLoading = true;
            try
            {
                var appointments = await _appointmentService.GetAppointmentsAsync();
                Appointments.Clear();
                foreach (var appointment in appointments)
                {
                    Appointments.Add(appointment);
                }
                FilterAppointments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterAppointments()
        {
            FilteredAppointments.Clear();

            var filtered = SelectedFilter switch
            {
                "Chờ xác nhận" => Appointments.Where(a => a.Status == "Chờ xác nhận"),
                "Đã xác nhận" => Appointments.Where(a => a.Status == "Đã xác nhận"),
                "Hoàn thành" => Appointments.Where(a => a.Status == "Hoàn thành"),
                "Đã hủy" => Appointments.Where(a => a.Status == "Đã hủy"),
                _ => Appointments
            };

            foreach (var appointment in filtered)
            {
                FilteredAppointments.Add(appointment);
            }
        }

        private async Task CancelAppointmentAsync(int appointmentId)
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Xác nhận",
                "Bạn có chắc muốn hủy lịch hẹn này?",
                "Có",
                "Không"
            );

            if (!confirm) return;

            string reason = await App.Current.MainPage.DisplayPromptAsync(
                "Lý do hủy",
                "Vui lòng nhập lý do hủy lịch:",
                "OK",
                "Hủy"
            );

            if (string.IsNullOrWhiteSpace(reason)) return;

            IsLoading = true;
            try
            {
                bool success = await _appointmentService.CancelAppointmentAsync(appointmentId, reason);
                if (success)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", "Đã hủy lịch hẹn", "OK");
                    await LoadAppointmentsAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Lỗi", "Không thể hủy lịch hẹn", "OK");
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