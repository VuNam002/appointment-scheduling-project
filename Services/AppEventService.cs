using System;

namespace ProjectMaui.Services
{
    public static class AppEventService
    {
        public static event EventHandler? AppointmentsChanged;

        public static void InvokeAppointmentsChanged()
        {
            // Make sure to invoke on the main thread if the handler updates the UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AppointmentsChanged?.Invoke(null, EventArgs.Empty);
            });
        }
    }
}