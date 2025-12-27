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
        if (BindingContext is AppointmentListViewModel viewModel && viewModel.FilteredAppointments.Count == 0)
        {
            viewModel.RefreshCommand.Execute(null);
        }
    }
}