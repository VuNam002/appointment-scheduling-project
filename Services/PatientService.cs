using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjectMaui.Models;

namespace ProjectMaui.Services
{
    public class PatientService : BaseService
    {
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
                            patients.Add(MapPatientFromReader(reader));
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
                                return MapPatientFromReader(reader);
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

        private PatientModel MapPatientFromReader(SqlDataReader reader)
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
