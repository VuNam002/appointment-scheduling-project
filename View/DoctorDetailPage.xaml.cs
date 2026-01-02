using ProjectMaui.ViewModels;
using ProjectMaui.Models;

namespace ProjectMaui.View;

public partial class DoctorDetailPage : ContentPage
{
    public DoctorDetailPage(DoctorDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
