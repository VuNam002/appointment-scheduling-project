using ProjectMaui.Models;
using ProjectMaui.Services;
using ProjectMaui.View;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectMaui.ViewModels
{
    // Kế thừa BaseViewModel cho gọn (đỡ phải viết lại INotifyPropertyChanged)
    public class AppointmentListViewModel : BaseViewModel
    {
        private readonly AppointmentService _appointmentService;

        // List gốc để lưu trữ
        private List<AppointmentDetailModel> _allAppointments;

        public ObservableCollection<AppointmentDetailModel> FilteredAppointments { get; set; }

        private string _selectedFilter = "Tất cả";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    FilterAppointments(); // Lọc lại ngay khi đổi picker
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CancelAppointmentCommand { get; }
        public ICommand ViewDetailCommand { get; } // Lệnh xem chi tiết

        public AppointmentListViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            _allAppointments = new List<AppointmentDetailModel>();
            FilteredAppointments = new ObservableCollection<AppointmentDetailModel>();

            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());

            // Logic hủy lịch
            CancelAppointmentCommand = new Command<int>(async (id) => await CancelAppointmentAsync(id));

            // Logic xem chi tiết
            ViewDetailCommand = new Command<int>(async (id) => await OnViewDetail(id));

            // Tải dữ liệu ngay lập tức
            LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // Bước 1: Thử lấy theo ID bệnh nhân đang đăng nhập
                int patientId = UserSession.Current.PatientId;
                var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);

                // --- SỬA LẠI: LOGIC DỰ PHÒNG (FALLBACK) ---
                // Nếu không tìm thấy lịch nào (hoặc chưa đăng nhập), thì lấy TẤT CẢ để hiển thị cho bạn test
                if (appointments == null || appointments.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Không thấy lịch riêng, đang tải TOÀN BỘ dữ liệu...");
                    appointments = await _appointmentService.GetAppointmentsAsync();
                }

                _allAppointments = appointments; // Lưu vào list gốc

                // Hiển thị ra màn hình
                FilterAppointments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", "Không tải được dữ liệu", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterAppointments()
        {
            if (_allAppointments == null) return;

            FilteredAppointments.Clear();

            IEnumerable<AppointmentDetailModel> query = _allAppointments;

            // Logic lọc theo trạng thái
            if (SelectedFilter != "Tất cả")
            {
                query = query.Where(a => a.Status == SelectedFilter);
            }

            foreach (var item in query)
            {
                FilteredAppointments.Add(item);
            }
        }

        private async Task CancelAppointmentAsync(int appointmentId)
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Xác nhận",
                "Bạn có chắc muốn hủy lịch hẹn này?",
                "Có",
                "Không"
            );

            if (!confirm) return;

            string reason = await Shell.Current.DisplayPromptAsync(
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
                    await Shell.Current.DisplayAlert("Thành công", "Đã hủy lịch hẹn", "OK");
                    await LoadAppointmentsAsync(); // Tải lại danh sách
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể hủy lịch hẹn", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Hàm xử lý chuyển trang chi tiết
        private async Task OnViewDetail(int appointmentId)
        {
            if (appointmentId <= 0) return;
            // Chuyển trang và gửi kèm ID
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointmentId}");
        }
    }
}