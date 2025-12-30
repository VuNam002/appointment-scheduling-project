using ProjectMaui.Models;
using ProjectMaui.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ProjectMaui.ViewModels
{
    public class AddDoctorViewModel : BaseViewModel
    {
        private string _doctorName;
        private string _phone;
        private string _email;
        private string _password;
        private string _specialization;
        private DepartmentModel _selectedDepartment;
        private DoctorScheduleModel _selectedDoctor;
        private ObservableCollection<DepartmentModel> _departments;
        private ObservableCollection<DoctorScheduleModel> _pendingSchedules;

        private readonly DepartmentService _departmentService;
        private readonly AuthService _authService;
        private readonly DoctorService _doctorService;

        public string DoctorName
        {
            get => _doctorName;
            set => SetProperty(ref _doctorName, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        public DepartmentModel SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }
        public DoctorScheduleModel SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        public ObservableCollection<DepartmentModel> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        // Danh sách lịch làm việc đang chờ thêm
        public ObservableCollection<DoctorScheduleModel> PendingSchedules
        {
            get => _pendingSchedules;
            set => SetProperty(ref _pendingSchedules, value);
        }

        // Danh sách ngày để hiển thị trong Picker
        public List<string> DaysList { get; } = new List<string> { "Chủ Nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7" };

        // Thêm các thuộc tính cho lịch làm việc
        private int _selectedDayIndex = 1; // Index trong DaysList (0=CN, 1=T2...)
        public int SelectedDayIndex
        {
            get => _selectedDayIndex;
            set => SetProperty(ref _selectedDayIndex, value);
        }

        private TimeSpan _startTime = new TimeSpan(8, 0, 0); // 08:00
        public TimeSpan StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private TimeSpan _endTime = new TimeSpan(17, 0, 0); // 17:00
        public TimeSpan EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public ICommand AddDoctorCommand { get; }
        public ICommand LoadDepartmentsCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand RemoveScheduleCommand { get; }


        public AddDoctorViewModel()
        {
            _departmentService = new DepartmentService();
            _authService = new AuthService();
            _doctorService = new DoctorService();
            Departments = new ObservableCollection<DepartmentModel>();
            PendingSchedules = new ObservableCollection<DoctorScheduleModel>();
            
            AddDoctorCommand = new Command(async () => await OnAddDoctor());
            LoadDepartmentsCommand = new Command(async () => await LoadDepartments());
            AddScheduleCommand = new Command(OnAddSchedule);
            RemoveScheduleCommand = new Command<DoctorScheduleModel>(OnRemoveSchedule);

            // Load departments when the view model is created
            Task.Run(async () => await LoadDepartments());
        }

        async Task LoadDepartments()
        {
            if (IsLoading)
                return;

            IsLoading = true;
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
            finally
            {
                IsLoading = false;
            }
        }

        private void OnAddSchedule()
        {
            if (StartTime >= EndTime)
            {
                Application.Current.MainPage.DisplayAlert("Lỗi", "Giờ kết thúc phải lớn hơn giờ bắt đầu", "OK");
                return;
            }

            // Tạo model lịch mới
            var schedule = new DoctorScheduleModel
            {
                DayOfWeek = SelectedDayIndex, // 0=CN, 1=T2... khớp với index của Picker
                StartTime = StartTime,
                EndTime = EndTime,
                // Giả sử Model có thuộc tính hiển thị, nếu không có thì UI sẽ tự format
                // DayOfWeekName = DaysList[SelectedDayIndex] 
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

        private async Task OnAddDoctor()
        {
            if (UserSession.Current.Role != "Admin")
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Bạn không có quyền thực hiện chức năng này.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(DoctorName) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                SelectedDepartment == null)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng điền đầy đủ thông tin.", "OK");
                return;
            }

            if (PendingSchedules.Count == 0)
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Xác nhận", "Bạn chưa thêm lịch làm việc nào. Bạn có chắc muốn tạo bác sĩ không?", "Có", "Không");
                if (!confirm) return;
            }

            IsLoading = true;

            // 1. Tạo bác sĩ và lấy ID
            var result = await _authService.RegisterDoctorAsync(DoctorName, Phone, Password, SelectedDepartment.DepartmentId, Specialization);

            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                // 2. Nếu thành công, thêm danh sách lịch làm việc cho bác sĩ đó
                foreach (var schedule in PendingSchedules)
                {
                    schedule.DoctorId = result.NewDoctorId;
                    await _doctorService.AddDoctorScheduleAsync(schedule);
                }

                IsLoading = false;
                await Application.Current.MainPage.DisplayAlert("Thành công", "Thêm bác sĩ và lịch làm việc thành công.", "OK");
                
                // Optionally, navigate back or clear the form
                DoctorName = string.Empty;
                Phone = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                Specialization = string.Empty;
                SelectedDepartment = null;
                PendingSchedules.Clear();
            }
            else
            {
                IsLoading = false;
                await Application.Current.MainPage.DisplayAlert("Lỗi", result.ErrorMessage, "OK");
            }
        }
    }
}