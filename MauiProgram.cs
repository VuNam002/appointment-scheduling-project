using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ProjectMaui.Data;
using ProjectMaui.Services;
using ProjectMaui.View;
using ProjectMaui.ViewModels;


namespace ProjectMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // --- 1. ĐĂNG KÝ SERVICES (Dịch vụ xử lý dữ liệu) ---
            builder.Services.AddSingleton<DoctorService>();
            builder.Services.AddSingleton<DepartmentService>();
            builder.Services.AddSingleton<PatientService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<AppointmentRepository>();
            builder.Services.AddSingleton<AppointmentService>(); 

            // --- 2. ĐĂNG KÝ VIEWMODELS (Logic giao diện) ---
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DoctorListViewModel>();
            builder.Services.AddTransient<AppointmentListViewModel>();
            builder.Services.AddTransient<AppointmentDetailViewModel>();
            builder.Services.AddTransient<DoctorDetailViewModel>();

            // Nếu bạn có các ViewModel này (dựa trên các file .xaml của bạn), hãy bỏ comment ra:
            // builder.Services.AddTransient<AddDoctorViewModel>();
            // builder.Services.AddTransient<BookingViewModel>();

            // --- 3. ĐĂNG KÝ PAGES (Giao diện) ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AppointmentListPage>();
            builder.Services.AddTransient<AppointmentDetailPage>();
            builder.Services.AddTransient<DoctorListPage>();
            builder.Services.AddTransient<DoctorDetailPage>();

            // Nếu bạn có các Page này, hãy bỏ comment ra:
            // builder.Services.AddTransient<AddDoctorPage>();
            // builder.Services.AddTransient<BookingPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}