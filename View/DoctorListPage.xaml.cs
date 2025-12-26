using ProjectMaui.Models;
using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class DoctorListPage : ContentPage
{
    public DoctorListPage(DoctorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnDoctorTapped(object sender, EventArgs e)
    {
        var frame = (Frame)sender;
        var doctor = (DoctorInfoModel)frame.BindingContext;

        // Hiệu ứng animation khi tap
        await frame.ScaleTo(0.95, 50);
        await frame.ScaleTo(1, 50);

        // Navigate to booking page with doctor info
        await Navigation.PushAsync(new BookingPage(doctor));
    }
}