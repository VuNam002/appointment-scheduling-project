using ProjectMaui.ViewModels;

namespace ProjectMaui.View;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}