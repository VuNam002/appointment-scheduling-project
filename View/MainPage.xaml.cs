using ProjectMaui.Services;

namespace ProjectMaui.View;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Hàm này chạy mỗi khi trang hiện lên
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Lấy tên từ UserSession để hiển thị
        if (UserSession.Current.IsLoggedIn)
        {
            lblWelcome.Text = UserSession.Current.FullName;
            lblPhone.Text = UserSession.Current.PhoneNumber;

            //// Cập nhật ảnh đại diện
            //if (!string.IsNullOrEmpty(UserSession.Current.Image))
            //{
            //    imgAvatar.Source = ImageSource.FromUri(new Uri(UserSession.Current.Image));
            //}
            //else
            //{
            //    imgAvatar.Source = "user_icon.png"; // Ảnh mặc định
            //}
        }
        else
        {
            lblWelcome.Text = "Khách vãng lai";
            lblPhone.Text = "Vui lòng đăng nhập";
            //imgAvatar.Source = "dotnet_bot.png"; // Reset về ảnh mặc định
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất?", "Có", "Không");
        if (answer)
        {
            // 1. Xóa dữ liệu phiên làm việc
            UserSession.Current.Clear();

            // 2. Chuyển hướng về trang Login
            // Dấu "///" giúp reset lại ngăn xếp điều hướng, người dùng không thể bấm Back để quay lại đây
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}