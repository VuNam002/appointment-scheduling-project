using ProjectMaui.Models;
using ProjectMaui.Services;
using ProjectMaui.View;
using ProjectMaui.ViewModels;
using System;

namespace ProjectMaui.View
{
    public partial class DoctorListPage : ContentPage
    {
        public DoctorListPage(DoctorListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnDoctorTapped(object sender, EventArgs e)
        {
            if (sender is not Frame frame || frame.BindingContext is not DoctorInfoModel doctor)
                return;

            await frame.ScaleTo(0.95, 50);
            await frame.ScaleTo(1, 50);

            if (UserSession.Current.Role == "Admin")
            {
                await Shell.Current.GoToAsync($"{nameof(DoctorDetailPage)}?doctorId={doctor.DoctorId}");
            }
            else
            {
                await Navigation.PushAsync(new BookingPage(doctor));
            }
        }
    }
}