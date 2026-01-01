// ViewModels/DoctorListViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using ProjectMaui.Models;
using ProjectMaui.Services;
using System;
using System.Threading.Tasks;

namespace ProjectMaui.ViewModels
{
    public class DoctorListViewModel : BaseViewModel 
    {
        private readonly DoctorService _doctorService;
        private readonly DepartmentService _departmentService;
        private readonly AuthService _authService;

        public ObservableCollection<DoctorInfoModel> Doctors { get; } = new();
        public ObservableCollection<DepartmentModel> Departments { get; } = new();

        private DepartmentModel _selectedDepartment;
        public DepartmentModel SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value) && value != null)
                {
                    _ = LoadDoctorsByDepartmentAsync(value.DepartmentId);
                }
            }
        }

        public ICommand LoadDoctorsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteDoctorCommand { get; }
        public bool IsAdmin => UserSession.Current.Role == "Admin";

        public DoctorListViewModel(DoctorService doctorService, DepartmentService departmentService, AuthService authService)
        {
            _doctorService = doctorService;
            _departmentService = departmentService;
            _authService = authService;
            Title = "Danh sách Bác sĩ";

            LoadDoctorsCommand = new Command(async () => await LoadDoctorsAsync());
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            DeleteDoctorCommand = new Command<DoctorInfoModel>(async (d) => await OnDeleteDoctor(d));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadDepartmentsAsync();
            await LoadDoctorsAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var doctors = await _doctorService.GetDoctorsAsync();
                Doctors.Clear();
                foreach (var d in doctors) Doctors.Add(d);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var depts = await _departmentService.GetDepartmentsAsync();
                Departments.Clear();
                Departments.Add(new DepartmentModel { DepartmentId = 0, DepartmentName = "Tất cả" });
                foreach (var dept in depts) Departments.Add(dept);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private async Task LoadDoctorsByDepartmentAsync(int deptId)
        {
            IsLoading = true;
            try
            {
                var doctors = deptId == 0
                    ? await _doctorService.GetDoctorsAsync()
                    : await _doctorService.GetDoctorsByDepartmentAsync(deptId);

                Doctors.Clear();
                foreach (var d in doctors) Doctors.Add(d);
            }
            finally { IsLoading = false; }
        }

        private async Task RefreshDataAsync() => await InitializeAsync();
        
        private async Task OnDeleteDoctor(DoctorInfoModel doctor)
        {
            if (doctor == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Xác nhận xóa",
                $"Bạn có chắc chắn muốn xóa bác sĩ '{doctor.DoctorName}' không? Hành động này không thể hoàn tác.",
                "Xóa", "Hủy");

            if (!confirm) return;

            IsLoading = true;
            try
            {
                var result = await _authService.SoftDeleteUserAsync(doctor.AccountId);
                if (result == null) // Success
                {
                    await Application.Current.MainPage.DisplayAlert("Thành công", "Đã xóa bác sĩ.", "OK");
                    // Efficiently remove from the list
                    if (Doctors.Contains(doctor))
                    {
                        Doctors.Remove(doctor);
                    }
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