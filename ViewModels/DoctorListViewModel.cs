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
    public class DoctorListViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<DoctorInfoModel> Doctors { get; set; }
        public ObservableCollection<DepartmentModel> Departments { get; set; }

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

        private DepartmentModel _selectedDepartment;
        public DepartmentModel SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                _selectedDepartment = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _ = LoadDoctorsByDepartmentAsync(value.DepartmentId);
                }
            }
        }

        public ICommand LoadDoctorsCommand { get; }
        public ICommand LoadDepartmentsCommand { get; }
        public ICommand RefreshCommand { get; }

        public DoctorListViewModel()
        {
            _databaseService = new DatabaseService();
            Doctors = new ObservableCollection<DoctorInfoModel>();
            Departments = new ObservableCollection<DepartmentModel>();

            LoadDoctorsCommand = new Command(async () => await LoadDoctorsAsync());
            LoadDepartmentsCommand = new Command(async () => await LoadDepartmentsAsync());
            RefreshCommand = new Command(async () => await RefreshDataAsync());

            // Load dữ liệu ban đầu
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadDepartmentsAsync();
            await LoadDoctorsAsync();
        }

        private async Task LoadDoctorsAsync()
        {
            IsLoading = true;
            try
            {
                var doctors = await _databaseService.GetDoctorsAsync();
                Doctors.Clear();
                foreach (var doctor in doctors)
                {
                    Doctors.Add(doctor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading doctors: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _databaseService.GetDepartmentsAsync();
                Departments.Clear();
                Departments.Add(new DepartmentModel { DepartmentId = 0, DepartmentName = "Tất cả" });
                foreach (var dept in departments)
                {
                    Departments.Add(dept);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading departments: {ex.Message}");
            }
        }

        private async Task LoadDoctorsByDepartmentAsync(int departmentId)
        {
            IsLoading = true;
            try
            {
                var doctors = departmentId == 0
                    ? await _databaseService.GetDoctorsAsync()
                    : await _databaseService.GetDoctorsByDepartmentAsync(departmentId);

                Doctors.Clear();
                foreach (var doctor in doctors)
                {
                    Doctors.Add(doctor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading doctors by department: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDataAsync()
        {
            if (SelectedDepartment != null && SelectedDepartment.DepartmentId > 0)
            {
                await LoadDoctorsByDepartmentAsync(SelectedDepartment.DepartmentId);
            }
            else
            {
                await LoadDoctorsAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}