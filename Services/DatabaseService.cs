// Services/DatabaseService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjectMaui.Models;
using ProjectMaui.Data;

namespace ProjectMaui.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Server=.;Database=ClinicDB;Integrated Security=True;TrustServerCertificate=True;";

        private AppointmentRepository _appointmentRepository;

        public DatabaseService()
        {
            _appointmentRepository = new AppointmentRepository(_connectionString);
        }

        #region Doctors

        public async Task<List<DoctorInfoModel>> GetDoctorsAsync()
        {
            var doctors = new List<DoctorInfoModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            d.DoctorId, d.DoctorName, d.Phone, d.Email, 
                            d.DepartmentId, d.Specialization, d.Image,
                            dept.DepartmentName, dept.Location
                        FROM Doctors d
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            doctors.Add(new DoctorInfoModel
                            {
                                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                DoctorName = reader["DoctorName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                DepartmentId = reader["DepartmentId"] == DBNull.Value ? null : Convert.ToInt32(reader["DepartmentId"]),
                                Specialization = reader["Specialization"]?.ToString() ?? "",
                                Image = reader["Image"]?.ToString() ?? "",
                                Department = new DepartmentModel
                                {
                                    DepartmentId = reader["DepartmentId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DepartmentId"]),
                                    DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                                    Location = reader["Location"]?.ToString() ?? ""
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDoctorsAsync: {ex.Message}");
            }
            return doctors;
        }

        public async Task<DoctorInfoModel> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            d.DoctorId, d.DoctorName, d.Phone, d.Email, 
                            d.DepartmentId, d.Specialization, d.Image,
                            dept.DepartmentName, dept.Location
                        FROM Doctors d
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId
                        WHERE d.DoctorId = @DoctorId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new DoctorInfoModel
                                {
                                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                    DoctorName = reader["DoctorName"]?.ToString() ?? "",
                                    Phone = reader["Phone"]?.ToString() ?? "",
                                    Email = reader["Email"]?.ToString() ?? "",
                                    DepartmentId = reader["DepartmentId"] == DBNull.Value ? null : Convert.ToInt32(reader["DepartmentId"]),
                                    Specialization = reader["Specialization"]?.ToString() ?? "",
                                    Image = reader["Image"]?.ToString() ?? "",
                                    Department = new DepartmentModel
                                    {
                                        DepartmentId = reader["DepartmentId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DepartmentId"]),
                                        DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                                        Location = reader["Location"]?.ToString() ?? ""
                                    }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDoctorByIdAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<List<DoctorInfoModel>> GetDoctorsByDepartmentAsync(int departmentId)
        {
            var doctors = new List<DoctorInfoModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT 
                            d.DoctorId, d.DoctorName, d.Phone, d.Email, 
                            d.DepartmentId, d.Specialization, d.Image,
                            dept.DepartmentName, dept.Location
                        FROM Doctors d
                        LEFT JOIN Departments dept ON d.DepartmentId = dept.DepartmentId
                        WHERE d.DepartmentId = @DepartmentId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentId", departmentId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                doctors.Add(new DoctorInfoModel
                                {
                                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                    DoctorName = reader["DoctorName"]?.ToString() ?? "",
                                    Phone = reader["Phone"]?.ToString() ?? "",
                                    Email = reader["Email"]?.ToString() ?? "",
                                    DepartmentId = reader["DepartmentId"] == DBNull.Value ? null : Convert.ToInt32(reader["DepartmentId"]),
                                    Specialization = reader["Specialization"]?.ToString() ?? "",
                                    Image = reader["Image"]?.ToString() ?? "",
                                    Department = new DepartmentModel
                                    {
                                        DepartmentId = Convert.ToInt32(reader["DepartmentId"]),
                                        DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                                        Location = reader["Location"]?.ToString() ?? ""
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDoctorsByDepartmentAsync: {ex.Message}");
            }
            return doctors;
        }

        #endregion

        #region Departments

        public async Task<List<DepartmentModel>> GetDepartmentsAsync()
        {
            var departments = new List<DepartmentModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT DepartmentId, DepartmentName, Location FROM Departments";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            departments.Add(new DepartmentModel
                            {
                                DepartmentId = Convert.ToInt32(reader["DepartmentId"]),
                                DepartmentName = reader["DepartmentName"]?.ToString() ?? "",
                                Location = reader["Location"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDepartmentsAsync: {ex.Message}");
            }
            return departments;
        }

        #endregion

        #region Patients

        public async Task<List<PatientModel>> GetPatientsAsync()
        {
            var patients = new List<PatientModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT PatientId, PatientName, Phone, DateOfBirth, Gender, Address FROM Patients";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            patients.Add(new PatientModel
                            {
                                PatientId = Convert.ToInt32(reader["PatientId"]),
                                PatientName = reader["PatientName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfBirth"]),
                                Gender = reader["Gender"]?.ToString() ?? "",
                                Address = reader["Address"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetPatientsAsync: {ex.Message}");
            }
            return patients;
        }

        public async Task<int> AddPatientAsync(PatientModel patient)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO Patients (PatientName, Phone, DateOfBirth, Gender, Address)
                        VALUES (@PatientName, @Phone, @DateOfBirth, @Gender, @Address);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PatientName", patient.PatientName);
                        command.Parameters.AddWithValue("@Phone", patient.Phone);
                        command.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Gender", patient.Gender ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Address", patient.Address ?? (object)DBNull.Value);

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi AddPatientAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<PatientModel> GetPatientByPhoneAsync(string phone)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT PatientId, PatientName, Phone, DateOfBirth, Gender, Address FROM Patients WHERE Phone = @Phone";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", phone);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new PatientModel
                                {
                                    PatientId = Convert.ToInt32(reader["PatientId"]),
                                    PatientName = reader["PatientName"]?.ToString() ?? "",
                                    Phone = reader["Phone"]?.ToString() ?? "",
                                    DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfBirth"]),
                                    Gender = reader["Gender"]?.ToString() ?? "",
                                    Address = reader["Address"]?.ToString() ?? ""
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetPatientByPhoneAsync: {ex.Message}");
            }
            return null;
        }

        #endregion

        #region Appointments

        public async Task<List<AppointmentDetailModel>> GetAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAppointmentsAsync();
        }

        public async Task<List<AppointmentDetailModel>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _appointmentRepository.GetAppointmentsByPatientAsync(patientId);
        }

        public async Task<int> CreateAppointmentAsync(AppointmentModel appointment)
        {
            // Kiểm tra bác sĩ có làm việc không
            bool isAvailable = await _appointmentRepository.CheckDoctorAvailabilityAsync(
                appointment.DoctorId,
                appointment.AppointmentDate
            );

            if (!isAvailable)
            {
                System.Diagnostics.Debug.WriteLine("Bác sĩ không làm việc vào thời gian này");
                return -1;
            }

            // Kiểm tra trùng lịch
            var existingAppointments = await _appointmentRepository.GetAppointmentsByDoctorAndDateAsync(
                appointment.DoctorId,
                appointment.AppointmentDate
            );

            if (existingAppointments.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Khung giờ này đã có người đặt");
                return -1;
            }

            return await _appointmentRepository.CreateAppointmentAsync(appointment);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status, string notes = null)
        {
            return await _appointmentRepository.UpdateStatusAsync(appointmentId, status, notes);
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
        {
            return await _appointmentRepository.UpdateStatusAsync(appointmentId, "Đã hủy", reason);
        }

        #endregion

        #region Doctor Schedule

        public async Task<List<DoctorScheduleModel>> GetDoctorScheduleAsync(int doctorId)
        {
            var schedules = new List<DoctorScheduleModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                        SELECT ScheduleId, DoctorId, DayOfWeek, StartTime, EndTime
                        FROM DoctorSchedule
                        WHERE DoctorId = @DoctorId
                        ORDER BY DayOfWeek, StartTime";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                schedules.Add(new DoctorScheduleModel
                                {
                                    ScheduleId = Convert.ToInt32(reader["ScheduleId"]),
                                    DoctorId = Convert.ToInt32(reader["DoctorId"]),
                                    DayOfWeek = Convert.ToInt32(reader["DayOfWeek"]),
                                    StartTime = (TimeSpan)reader["StartTime"],
                                    EndTime = (TimeSpan)reader["EndTime"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GetDoctorScheduleAsync: {ex.Message}");
            }
            return schedules;
        }

        #endregion
    }
}