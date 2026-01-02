using ProjectMaui.Services;
using ProjectMaui.View;

namespace ProjectMaui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AppointmentDetailPage), typeof(AppointmentDetailPage));
            Routing.RegisterRoute(nameof(AddDoctorPage), typeof(AddDoctorPage));
            Routing.RegisterRoute(nameof(DoctorDetailPage), typeof(DoctorDetailPage));
            
        }
        public void SetupTabsForRole()
        {
            var role = UserSession.Current.Role;

            if (role == "Patient" || role == "Admin")
            {
                DoctorListTab.IsEnabled = true;
                DoctorListTab.IsVisible = true;
            }
            else
            {
                DoctorListTab.IsEnabled = false;
                DoctorListTab.IsVisible = false;
            }
        }
    }
}
