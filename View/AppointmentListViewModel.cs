using ProjectMaui.Models;
using ProjectMaui.Services;
using ProjectMaui.View;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ProjectMaui.ViewModels;

namespace ProjectMaui.ViewModels
{
    public class AppointmentListViewModel : BaseViewModel
    {
        private readonly AppointmentService _appointmentService;
        private List<AppointmentDetailModel> _allAppointments;
        public ObservableCollection<AppointmentDetailModel> FilteredAppointments { get; set; }

        // Bộ lọc trạng thái
        private string _selectedFilter = "Tất cả";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    FilterAppointments(); 
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand CancelAppointmentCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand UpdateStatusCommand { get; }

        public AppointmentListViewModel()
        {
            _appointmentService = new AppointmentService();

            // Khởi tạo 2 danh sách để tránh lỗi Null
            _allAppointments = new List<AppointmentDetailModel>();
            FilteredAppointments = new ObservableCollection<AppointmentDetailModel>();

            RefreshCommand = new Command(async () => await LoadAppointmentsAsync());
            CancelAppointmentCommand = new Command<int>(async (id) => await CancelAppointmentAsync(id));
            ViewDetailCommand = new Command<int>(async (id) => await OnViewDetail(id));
            UpdateStatusCommand = new Command<AppointmentDetailModel>(async (model) => await UpdateAppointmentStatusAsync(model));

            // Subscribe to event to refresh list
            AppEventService.AppointmentsChanged += HandleAppointmentsChanged;
        }

        private void HandleAppointmentsChanged(object? sender, EventArgs e)
        {
            if (RefreshCommand.CanExecute(null))
            {
                RefreshCommand.Execute(null);
            }
        }

        // Finalizer to unsubscribe and prevent memory leaks
        ~AppointmentListViewModel()
        {
            AppEventService.AppointmentsChanged -= HandleAppointmentsChanged;
        }
        // --- HÀM TẢI DỮ LIỆU ---
        private async Task LoadAppointmentsAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // Gọi hàm thông minh: Admin lấy hết, Bác sĩ chỉ lấy lịch của mình
                var listData = await _appointmentService.GetMyScheduleAsync();

                // Lưu vào danh sách gốc
                _allAppointments = listData ?? new List<AppointmentDetailModel>();

                // Gọi hàm lọc để đẩy dữ liệu ra màn hình
                FilterAppointments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading: {ex.Message}");
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

            // Xóa danh sách hiển thị cũ
            FilteredAppointments.Clear();

            IEnumerable<AppointmentDetailModel> query = _allAppointments;

            if (SelectedFilter != "Tất cả")
            {
                query = query.Where(a => a.Status == SelectedFilter);
            }
            foreach (var item in query)
            {
                FilteredAppointments.Add(item);
            }
        }

        // --- HÀM HỦY LỊCH ---
        private async Task CancelAppointmentAsync(int appointmentId)
        {
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn hủy lịch này?", "Có", "Không");
            if (!confirm) return;

            string reason = await Shell.Current.DisplayPromptAsync("Lý do", "Nhập lý do hủy:", "OK", "Hủy");
            if (string.IsNullOrWhiteSpace(reason)) return;

            IsLoading = true;
            try
            {
                bool success = await _appointmentService.CancelAppointmentAsync(appointmentId, reason);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Thành công", "Đã hủy lịch hẹn", "OK");
                    
                    // Cập nhật trạng thái ngay trên UI thay vì tải lại toàn bộ
                    var appointmentToUpdate = _allAppointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
                    if (appointmentToUpdate != null)
                    {
                        appointmentToUpdate.Status = "Đã hủy";
                        appointmentToUpdate.Reason = reason;
                        FilterAppointments(); // Áp dụng lại bộ lọc để cập nhật UI
                    }
                    else
                    {
                        await LoadAppointmentsAsync(); // Tải lại nếu không tìm thấy
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể hủy lịch", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // --- HÀM XEM CHI TIẾT ---
        private async Task UpdateAppointmentStatusAsync(AppointmentDetailModel appointment)
        {
            if (appointment == null) return;

            string[] possibleStatuses = GetPossibleNextStatuses(appointment.Status);

            if (possibleStatuses.Length == 0)
            {
                await Shell.Current.DisplayAlert("Thông báo", "Không có hành động nào cho trạng thái này.", "OK");
                return;
            }

            string newStatus = await Shell.Current.DisplayActionSheet("Cập nhật trạng thái", "Hủy", null, possibleStatuses);

            if (string.IsNullOrEmpty(newStatus) || newStatus == "Hủy")
            {
                return;
            }

            string notes = appointment.Reason; 
            if (newStatus == "Đã hủy")
            {
                notes = await Shell.Current.DisplayPromptAsync("Lý do", "Nhập lý do hủy:", "OK", "Hủy", initialValue: appointment.Reason);
                if (notes == null) return; 
            }

            IsLoading = true;
            try
            {
                bool success = await _appointmentService.UpdateAppointmentStatusAsync(appointment.AppointmentId, newStatus, notes);
                if (success)
                {
                    await Shell.Current.DisplayAlert("Thành công", "Đã cập nhật trạng thái.", "OK");
                    appointment.Status = newStatus;
                    appointment.Reason = notes;
                    FilterAppointments();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể cập nhật trạng thái.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string[] GetPossibleNextStatuses(string currentStatus)
        {
            return currentStatus switch
            {
                "Chờ xác nhận" => new[] { "Đã xác nhận", "Đã hủy" },
                "Đã xác nhận" => new[] { "Đã hoàn thành", "Đã hủy" },
                _ => Array.Empty<string>()
            };
        }

        private async Task OnViewDetail(int appointmentId)
        {
            if (appointmentId <= 0) return;
            // Chuyển sang trang chi tiết
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointmentId}");
        }
    }
}