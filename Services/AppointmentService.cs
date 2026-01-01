using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectMaui.Models;
using ProjectMaui.Data;

namespace ProjectMaui.Services
{
    public class AppointmentService : BaseService
    {
        private readonly AppointmentRepository _appointmentRepository;

        public AppointmentService()
        {
            _appointmentRepository = new AppointmentRepository(_connectionString);
        }

        public async Task<List<AppointmentDetailModel>> GetAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAppointmentsAsync();
        }

        public async Task<List<AppointmentDetailModel>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _appointmentRepository.GetAppointmentsByPatientAsync(patientId);
        }

        public async Task<(string ErrorMessage, int AppointmentId)> CreateAppointmentAsync(AppointmentModel appointment)
        {
            bool isAvailable = await _appointmentRepository.CheckDoctorAvailabilityAsync(
                appointment.DoctorId,
                appointment.AppointmentDate
            );

            if (!isAvailable)
            {
                System.Diagnostics.Debug.WriteLine("Bác sĩ không làm việc vào thời gian này");
                return ("Bác sĩ không có lịch làm việc vào thời gian này. Vui lòng chọn khung giờ khác.", 0);
            }

            // Kiểm tra trùng lịch
            var existingAppointments = await _appointmentRepository.GetAppointmentsByDoctorAndDateAsync(
                appointment.DoctorId,
                appointment.AppointmentDate
            );

            if (existingAppointments.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Khung giờ này đã có người đặt");
                return ("Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.", 0);
            }

            int newId = await _appointmentRepository.CreateAppointmentAsync(appointment);
            if (newId > 0)
            {
                return (null, newId);
            }
            return ("Không thể tạo lịch hẹn do lỗi hệ thống.", 0);
        }

        public async Task<AppointmentDetailModel> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status, string notes = null)
        {
            return await _appointmentRepository.UpdateStatusAsync(appointmentId, status, notes);
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            return await _appointmentRepository.UpdateStatusAsync(appointmentId, "Đã hủy", reason);
        }
        public async Task<List<AppointmentDetailModel>> GetMyScheduleAsync()
        {
            string currentRole = UserSession.Current.Role;
            int currentDoctorId = UserSession.Current.DoctorId;
            int currentPatientId = UserSession.Current.PatientId;
            return await _appointmentRepository.GetAppointmentsByRoleAsync(currentRole, currentDoctorId, currentPatientId);
        }
    }
}
