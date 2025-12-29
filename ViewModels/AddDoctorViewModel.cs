using ProjectMaui.Models;
using ProjectMaui.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private ObservableCollection<DepartmentModel> _departments;

        private readonly DepartmentService _departmentService;
        private readonly AuthService _authService;

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

        public ObservableCollection<DepartmentModel> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ICommand AddDoctorCommand { get; }
        public ICommand LoadDepartmentsCommand { get; }


        public AddDoctorViewModel()
        {
            _departmentService = new DepartmentService();
            _authService = new AuthService();
            Departments = new ObservableCollection<DepartmentModel>();
            AddDoctorCommand = new Command(async () => await OnAddDoctor());
            LoadDepartmentsCommand = new Command(async () => await LoadDepartments());

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
                Departments.Clear();
                foreach (var dept in departmentsList)
                {
                    Departments.Add(dept);
                }
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

            IsLoading = true;

            var errorMessage = await _authService.RegisterDoctorAsync(DoctorName, Phone, Email, Password, SelectedDepartment.DepartmentId, Specialization);

            IsLoading = false;

            if (string.IsNullOrEmpty(errorMessage))
            {
                await Application.Current.MainPage.DisplayAlert("Thành công", "Thêm bác sĩ thành công.", "OK");
                // Optionally, navigate back or clear the form
                DoctorName = string.Empty;
                Phone = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                Specialization = string.Empty;
                SelectedDepartment = null;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", errorMessage, "OK");
            }
        }
    }
}