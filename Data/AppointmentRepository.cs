using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjectMaui.Models;

namespace ProjectMaui.Data
{
    public class AppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<AppointmentDetailModel>> GetAllAppointmentsAsync()
        {
            var appointments = new List<AppointmentDetailModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            a.AppointmentId, a.AppointmentDate, a.Status, a.Reason, a.Notes,
                            d.DoctorId, d.DoctorName, d.Phone AS DoctorPhone, d.Email AS DoctorEmail,
                            d.Specialization, d.Image AS DoctorImage,
                            p.PatientId, p.PatientName, p.Phone AS PatientPhone, p.Address AS PatientAddress,
                            dept.DepartmentName, dept.Location AS DepartmentLocation
                        FROM Appointments a
                        INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
                        INNER JOIN Patients p ON a.PatientId = p.PatientId
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId
                        ORDER BY a.AppointmentDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            appointments.Add(MapToDetailModel(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAllAppointmentsAsync: {ex.Message}");
            }
            return appointments;
        }

        public async Task<List<AppointmentDetailModel>> GetAppointmentsByPatientAsync(int patientId)
        {
            var appointments = new List<AppointmentDetailModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            a.AppointmentId, a.AppointmentDate, a.Status, a.Reason, a.Notes,
                            d.DoctorId, d.DoctorName, d.Phone AS DoctorPhone, d.Email AS DoctorEmail,
                            d.Specialization, d.Image AS DoctorImage,
                            p.PatientId, p.PatientName, p.Phone AS PatientPhone, p.Address AS PatientAddress,
                            dept.DepartmentName, dept.Location AS DepartmentLocation
                        FROM Appointments a
                        INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
                        INNER JOIN Patients p ON a.PatientId = p.PatientId
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId
                        WHERE a.PatientId = @PatientId
                        ORDER BY a.AppointmentDate DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PatientId", patientId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                appointments.Add(MapToDetailModel(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAppointmentsByPatientAsync: {ex.Message}");
            }
            return appointments;
        }

        public async Task<int> CreateAppointmentAsync(AppointmentModel appointment)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO Appointments (DoctorId, PatientId, AppointmentDate, Status, Reason, Notes)
                        VALUES (@DoctorId, @PatientId, @AppointmentDate, @Status, @Reason, @Notes);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                        command.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                        command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                        command.Parameters.AddWithValue("@Status", appointment.Status ?? "Chờ xác nhận");
                        command.Parameters.AddWithValue("@Reason", appointment.Reason ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Notes", appointment.Notes ?? (object)DBNull.Value);

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CreateAppointmentAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateStatusAsync(int appointmentId, string status, string notes = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        UPDATE Appointments 
                        SET Status = @Status, Notes = @Notes
                        WHERE AppointmentId = @AppointmentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error UpdateStatusAsync: {ex.Message}");
                return false;
            }
        }
        public async Task<AppointmentDetailModel> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            a.AppointmentId, a.AppointmentDate, a.Status, a.Reason, a.Notes,
                            d.DoctorId, d.DoctorName, d.Phone AS DoctorPhone, d.Email AS DoctorEmail,
                            d.Specialization, d.Image AS DoctorImage,
                            p.PatientId, p.PatientName, p.Phone AS PatientPhone, p.Address AS PatientAddress,
                            dept.DepartmentName, dept.Location AS DepartmentLocation
                        FROM Appointments a
                        INNER JOIN Doctors d ON a.DoctorId = d.DoctorId
                        INNER JOIN Patients p ON a.PatientId = p.PatientId
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId
                        WHERE a.AppointmentId = @AppointmentId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return MapToDetailModel(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAppointmentByIdAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> CheckDoctorAvailabilityAsync(int doctorId, DateTime appointmentDate)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Lấy thứ trong tuần (2-8)
                    int dayOfWeek = (int)appointmentDate.DayOfWeek + 1;
                    if (dayOfWeek == 1) dayOfWeek = 8; // Chủ nhật = 8

                    string query = @"
                        SELECT COUNT(*) 
                        FROM DoctorSchedule 
                        WHERE DoctorId = @DoctorId 
                            AND DayOfWeek = @DayOfWeek 
                            AND @Time >= StartTime 
                            AND @Time <= EndTime";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);
                        command.Parameters.AddWithValue("@DayOfWeek", dayOfWeek);
                        command.Parameters.AddWithValue("@Time", appointmentDate.TimeOfDay);

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error CheckDoctorAvailabilityAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<AppointmentModel>> GetAppointmentsByDoctorAndDateAsync(int doctorId, DateTime appointmentDate)
        {
            var appointments = new List<AppointmentModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT AppointmentId, DoctorId, PatientId, AppointmentDate, Status, Reason, Notes
                        FROM Appointments
                        WHERE DoctorId = @DoctorId 
                            AND AppointmentDate = @AppointmentDate
                            AND Status NOT IN ('Đã hủy')";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);
                        command.Parameters.AddWithValue("@AppointmentDate", appointmentDate);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                appointments.Add(new AppointmentModel
                                {
                                    AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                    PatientId = Convert.ToInt32(reader["PatientId"]),
                                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                                    Status = reader["Status"]?.ToString() ?? "",
                                    Reason = reader["Reason"]?.ToString() ?? "",
                                    Notes = reader["Notes"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAppointmentsByDoctorAndDateAsync: {ex.Message}");
            }
            return appointments;
        }

        private AppointmentDetailModel MapToDetailModel(SqlDataReader reader)
        {
            return new AppointmentDetailModel
            {
                AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                Status = reader["Status"]?.ToString() ?? "",
                Reason = reader["Reason"]?.ToString() ?? "",
                Notes = reader["Notes"]?.ToString() ?? "",
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                DoctorName = reader["DoctorName"]?.ToString() ?? "",
                DoctorPhone = reader["DoctorPhone"]?.ToString() ?? "",
                DoctorEmail = reader["DoctorEmail"]?.ToString() ?? "",
                Specialization = reader["Specialization"]?.ToString() ?? "",
                DoctorImage = reader["DoctorImage"]?.ToString() ?? "",
                PatientId = Convert.ToInt32(reader["PatientId"]),
                PatientName = reader["PatientName"]?.ToString() ?? "",
                PatientPhone = reader["PatientPhone"]?.ToString() ?? "",
                PatientAddress = reader["PatientAddress"]?.ToString() ?? "",
                DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                DepartmentLocation = reader["DepartmentLocation"]?.ToString() ?? ""
            };
        }
    }
}