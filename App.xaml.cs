// App.xaml.cs: Code-behind của App.xaml, nơi khởi tạo trang chính
using Microsoft.Extensions.DependencyInjection;
namespace ProjectMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}