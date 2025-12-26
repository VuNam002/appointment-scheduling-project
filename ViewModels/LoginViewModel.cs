using ProjectMaui.Services;
using System.Windows.Input;

namespace ProjectMaui.ViewModels
{
    public class LoginViewModel : BaseViewModel // Kế thừa BaseViewModel có sẵn INotifyPropertyChanged
    {
        private readonly AuthService _authService;

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await OnLogin());
            GoToRegisterCommand = new Command(async () => await OnRegister());
        }

        private async Task OnLogin()
        {
            if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                // Gọi Service kiểm tra DB
                bool isSuccess = await _authService.LoginAsync(Phone, Password);

                if (isSuccess)
                {
                    await App.Current.MainPage.DisplayAlert("Thành công", $"Xin chào {UserSession.Current.FullName}!", "OK");
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Thất bại", "Sai số điện thoại hoặc mật khẩu", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Lỗi hệ thống", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnRegister()
        {
            await App.Current.MainPage.DisplayAlert("Thông báo", "Chức năng đăng ký đang phát triển", "OK");
            // Sau này sẽ: await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}