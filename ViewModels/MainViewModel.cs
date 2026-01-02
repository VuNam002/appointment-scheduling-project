using ProjectMaui.Services;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace ProjectMaui.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private readonly AuthService _authService;

        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public ICommand ChangePasswordCommand { get; }

        public MainViewModel()
        {
            _authService = new AuthService();
            ChangePasswordCommand = new Command(async () => await OnChangePassword());
        }

        private async Task OnChangePassword()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin.", "OK");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Mật khẩu mới và xác nhận mật khẩu không khớp.", "OK");
                return;
            }

            IsLoading = true;
            var result = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);
            IsLoading = false;

            if (result == null)
            {
                await Application.Current.MainPage.DisplayAlert("Thành công", "Đổi mật khẩu thành công.", "OK");
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", result, "OK");
            }
        }
    }
}
