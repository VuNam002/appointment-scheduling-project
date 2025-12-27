using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class AddDoctorPage : ContentPage
{
	public AddDoctorPage()
	{
		InitializeComponent();
		this.BindingContext = new AddDoctorViewModel();
	}
}