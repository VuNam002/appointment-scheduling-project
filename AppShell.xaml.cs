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
            
        }
        public void SetupTabsForRole()
        {
            var role = UserSession.Current.Role;
            // Assuming "Admin" and "Patient" are the roles that can see the doctor list.
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
