using Microsoft.Extensions.Logging;
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

            // --- 1. ĐĂNG KÝ SERVICES ---
            builder.Services.AddSingleton<DoctorService>();
            builder.Services.AddSingleton<DepartmentService>();
            builder.Services.AddSingleton<PatientService>();
            builder.Services.AddSingleton<AppointmentService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<AppointmentRepository>();
            builder.Services.AddSingleton<AppointmentService>();

            // --- 2. ĐĂNG KÝ VIEWMODELS ---
            builder.Services.AddTransient<DoctorListViewModel>();
            builder.Services.AddTransient<AppointmentListViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AppointmentDetailViewModel>();
            builder.Services.AddTransient<AppointmentDetailPage>();

            // --- 3. ĐĂNG KÝ PAGES ---
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AppointmentListPage>();
            builder.Services.AddTransient<DoctorListPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}