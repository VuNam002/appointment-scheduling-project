using ProjectMaui.Models;
using ProjectMaui.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProjectMaui.ViewModels
{
    [QueryProperty(nameof(AppointmentId), "id")]
    public class AppointmentDetailViewModel : BaseViewModel
    {
        private readonly AppointmentService _appointmentService;

        private int _appointmentId;
        public int AppointmentId
        {
            get => _appointmentId;
            set
            {
                _appointmentId = value;
                LoadAppointmentDetail();
            }
        }

        private AppointmentDetailModel _detail;
        private List<AppointmentDetailModel> _allAppointments;

        public AppointmentDetailModel Detail
        {
            get => _detail;
            set => SetProperty(ref _detail, value);
        }

        public ICommand CancelCommand { get; }
        public ICommand BackCommand { get; }

        public AppointmentDetailViewModel(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;

            CancelCommand = new Command(async () => await OnCancel());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async void LoadAppointmentDetail()
        {
            IsLoading = true;
            try
            {
                var data = await _appointmentService.GetAppointmentByIdAsync(AppointmentId);
                Detail = data;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không tải được dữ liệu: " + ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnCancel()
        {
            if (Detail == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn chắc chắn muốn hủy lịch hẹn này?", "Đồng ý", "Không");
            if (confirm)
            {
                IsLoading = true;
                // Gọi Service để hủy
                bool success = await _appointmentService.CancelAppointmentAsync(AppointmentId, "Người dùng hủy qua App");

                if (success)
                {
                    await Shell.Current.DisplayAlert("Thành công", "Đã hủy lịch hẹn", "OK");
                    // Tải lại để cập nhật trạng thái mới (Đã hủy)
                    LoadAppointmentDetail();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Thất bại", "Không thể hủy lịch, vui lòng thử lại", "OK");
                }
                IsLoading = false;
            }
        }
        //private async Task LoadAppointmentsAsync()
        //{
        //    if (IsLoading) return;
        //    IsLoading = true;

        //    try
        //    {
        //        var listData = await _appointmentService.GetMyScheduleAsync();
        //        _allAppointments = listData ?? new List<AppointmentDetailModel>();
        //        FilterAppointments();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
        //        await Shell.Current.DisplayAlert("Lỗi", "Không tải được dữ liệu", "OK");
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}

        //private void FilterAppointments()
        //{
        //    throw new NotImplementedException();
        //}
    }
}