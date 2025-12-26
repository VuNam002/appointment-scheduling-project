using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjectMaui.Models;

namespace ProjectMaui.Services
{
    public class DoctorService : BaseService
    {
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
                            doctors.Add(MapDoctorFromReader(reader));
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
                                return MapDoctorFromReader(reader);
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
                                doctors.Add(MapDoctorFromReader(reader));
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

        private DoctorInfoModel MapDoctorFromReader(SqlDataReader reader)
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
