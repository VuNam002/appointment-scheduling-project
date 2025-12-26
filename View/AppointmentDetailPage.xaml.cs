using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class AppointmentDetailPage : ContentPage
{
    public AppointmentDetailPage(AppointmentDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}