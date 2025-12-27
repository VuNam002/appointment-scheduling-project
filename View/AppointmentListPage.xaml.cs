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
            App.Current.MainPage?.DisplayAlert("Lỗi Giao Diện", ex.Message, "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AppointmentListViewModel viewModel)
        {
            // Always refresh data when the page appears to ensure it's up-to-date
            // for the current user.
            viewModel.RefreshCommand.Execute(null);
        }
    }
}