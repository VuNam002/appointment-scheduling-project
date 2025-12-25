using ProjectMaui.Models;
using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class BookingPage : ContentPage
{
    public BookingPage(DoctorInfoModel doctor)
    {
        InitializeComponent();
        BindingContext = new BookingViewModel(doctor);
    }
}