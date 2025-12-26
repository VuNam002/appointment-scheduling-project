using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class AppointmentListPage : ContentPage
{
    public AppointmentListPage(AppointmentListViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CRASH TẠI APPOINTMENT LIST: {ex.Message}");
            // Nếu chạy trên Windows, dòng này sẽ hiện popup lỗi
            App.Current.MainPage?.DisplayAlert("Lỗi Giao Diện", ex.Message, "OK");
        }
    }
}