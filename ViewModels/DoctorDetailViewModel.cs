using ProjectMaui.Models;
using ProjectMaui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace ProjectMaui.ViewModels
{
    [QueryProperty(nameof(DoctorId), "doctorId")]
    public class DoctorDetailViewModel : BaseViewModel
    {
        private int _doctorId;
        private string _doctorName;
        private string _phone;
        private string _email;
        private string _specialization;
        private DepartmentModel _selectedDepartment;
        private ObservableCollection<DepartmentModel> _departments;
        private ObservableCollection<DoctorScheduleModel> _pendingSchedules;
        private DoctorInfoModel _currentDoctor;

        private readonly DepartmentService _departmentService;
        private readonly AuthService _authService;
        private readonly DoctorService _doctorService;

        public int DoctorId
        {
            get => _doctorId;
            set
            {
                _doctorId = value;
                if (_doctorId > 0)
                {
                    Task.Run(() => LoadDoctorDetails());
                }
            }
        }

        public string DoctorName { get => _doctorName; set => SetProperty(ref _doctorName, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Specialization { get => _specialization; set => SetProperty(ref _specialization, value); }
        public DepartmentModel SelectedDepartment { get => _selectedDepartment; set => SetProperty(ref _selectedDepartment, value); }
        public ObservableCollection<DepartmentModel> Departments { get => _departments; set => SetProperty(ref _departments, value); }
        public ObservableCollection<DoctorScheduleModel> PendingSchedules { get => _pendingSchedules; set => SetProperty(ref _pendingSchedules, value); }

        public List<string> DaysList { get; } = new List<string> { "Chủ Nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };
        private string _selectedDay = "Thứ 2";
        public string SelectedDay { get => _selectedDay; set => SetProperty(ref _selectedDay, value); }

        private TimeSpan _startTime = new TimeSpan(8, 0, 0);
        public TimeSpan StartTime { get => _startTime; set => SetProperty(ref _startTime, value); }

        private TimeSpan _endTime = new TimeSpan(17, 0, 0);
        public TimeSpan EndTime { get => _endTime; set => SetProperty(ref _endTime, value); }

        public ICommand UpdateDoctorCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand RemoveScheduleCommand { get; }
        public ICommand DeleteDoctorCommand { get; }
        public bool IsAdmin => UserSession.Current.Role == "Admin";

        public DoctorDetailViewModel()
        {
            _departmentService = new DepartmentService();
            _authService = new AuthService();
            _doctorService = new DoctorService();
            Departments = new ObservableCollection<DepartmentModel>();
            PendingSchedules = new ObservableCollection<DoctorScheduleModel>();

            UpdateDoctorCommand = new Command(async () => await OnUpdateDoctor());
            AddScheduleCommand = new Command(OnAddSchedule);
            RemoveScheduleCommand = new Command<DoctorScheduleModel>(OnRemoveSchedule);
            DeleteDoctorCommand = new Command(async () => await OnDeleteDoctor());

            Task.Run(async () => await LoadDepartments());
        }

        async Task LoadDoctorDetails()
        {
            if (DoctorId == 0) return;

            IsLoading = true;
            try
            {
                _currentDoctor = await _doctorService.GetDoctorByIdAsync(DoctorId);
                if (_currentDoctor != null)
                {
                    DoctorName = _currentDoctor.DoctorName;
                    Phone = _currentDoctor.Phone;
                    Email = _currentDoctor.Email;
                    Specialization = _currentDoctor.Specialization;
                    
                    var schedules = await _doctorService.GetDoctorScheduleAsync(DoctorId);
                    PendingSchedules = new ObservableCollection<DoctorScheduleModel>(schedules);

                    await LoadDepartments();
                    SelectedDepartment = Departments.FirstOrDefault(d => d.DepartmentId == _currentDoctor.DepartmentId);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải thông tin bác sĩ: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        async Task LoadDepartments()
        {
            try
            {
                var departmentsList = await _departmentService.GetDepartmentsAsync();
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    Departments.Clear();
                    foreach (var dept in departmentsList)
                    {
                        Departments.Add(dept);
                    }
                });
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải danh sách khoa: {ex.Message}", "OK");
            }
        }

        private void OnAddSchedule()
        {
            if (StartTime >= EndTime)
            {
                Application.Current.MainPage.DisplayAlert("Lỗi", "Giờ kết thúc phải lớn hơn giờ bắt đầu.", "OK");
                return;
            }

            var schedule = new DoctorScheduleModel
            {
                DoctorId = this.DoctorId,
                DayOfWeek = DaysList.IndexOf(SelectedDay),
                StartTime = StartTime,
                EndTime = EndTime,
            };

            PendingSchedules.Add(schedule);
        }

        private void OnRemoveSchedule(DoctorScheduleModel schedule)
        {
            if (PendingSchedules.Contains(schedule))
            {
                PendingSchedules.Remove(schedule);
            }
        }

        private async Task OnUpdateDoctor()
        {
            if (!IsAdmin)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Bạn không có quyền thực hiện chức năng này.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(DoctorName) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Email) || SelectedDepartment == null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng điền đầy đủ thông tin.", "OK");
                return;
            }

            IsLoading = true;

            var doctorToUpdate = new DoctorInfoModel
            {
                DoctorId = this.DoctorId,
                DoctorName = this.DoctorName,
                Email = this.Email,
                Phone = this.Phone,
                DepartmentId = this.SelectedDepartment.DepartmentId,
                Specialization = this.Specialization,
                Schedules = new List<DoctorScheduleModel>(this.PendingSchedules)
            };

            var result = await _authService.UpdateDoctorAsync(doctorToUpdate);

            if (string.IsNullOrEmpty(result))
            {
                await Application.Current.MainPage.DisplayAlert("Thành công", "Cập nhật thông tin bác sĩ thành công.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", result, "OK");
            }

            IsLoading = false;
        }

        private async Task OnDeleteDoctor()
        {
            if (_currentDoctor == null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không có thông tin bác sĩ để xóa.", "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa bác sĩ '{_currentDoctor.DoctorName}' không? Hành động này không thể hoàn tác.",
                "Xóa", "Hủy");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var result = await _authService.SoftDeleteUserAsync(_currentDoctor.AccountId);
                if (string.IsNullOrEmpty(result))
                {
                    await Application.Current.MainPage.DisplayAlert("Thành công", "Đã xóa bác sĩ.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", result, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi OnDeleteDoctor: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Đã xảy ra lỗi khi xóa bác sĩ.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
