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
            
        }
        public void SetupTabsForRole()
        {
            var role = UserSession.Current.Role;
            // The doctor list should be visible to all logged-in roles.
            if (role == "Patient" || role == "Admin" || role == "Doctor")
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
