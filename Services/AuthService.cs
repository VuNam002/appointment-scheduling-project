using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ProjectMaui.Services
{
    // 1. Thêm kế thừa : BaseService
    public class AuthService : BaseService
    {
        // 2. KHÔNG CẦN khai báo private string _connectionString nữa
        // Vì class cha (BaseService) đã có biến _connectionString dạng 'protected' rồi.

        public AuthService()
        {
        }

        public async Task<bool> LoginAsync(string phone, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                        SELECT 
                            a.AccountId, 
                            a.PhoneNumber, 
                            r.RoleName,
                            p.PatientId, p.PatientName,
                            d.DoctorId, d.DoctorName, d.Image
                        FROM Accounts a
                        JOIN Roles r ON a.RoleId = r.RoleId
                        LEFT JOIN Patients p ON a.AccountId = p.AccountId
                        LEFT JOIN Doctors d ON a.AccountId = d.AccountId
                        WHERE a.PhoneNumber = @Phone 
                          AND a.PasswordHash = @Password 
                          AND a.IsActive = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                UserSession.Current.Clear();
                                UserSession.Current.AccountId = Convert.ToInt32(reader["AccountId"]);
                                UserSession.Current.PhoneNumber = reader["PhoneNumber"].ToString();
                                UserSession.Current.Role = reader["RoleName"].ToString();

                                if (UserSession.Current.Role == "Patient")
                                {
                                    UserSession.Current.PatientId = reader["PatientId"] != DBNull.Value ? Convert.ToInt32(reader["PatientId"]) : 0;
                                    UserSession.Current.FullName = reader["PatientName"]?.ToString();
                                }
                                else if (UserSession.Current.Role == "Doctor")
                                {
                                    UserSession.Current.DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : 0;
                                    UserSession.Current.FullName = reader["DoctorName"]?.ToString();
                                    UserSession.Current.Image = reader["Image"]?.ToString();
                                }
                                else if (UserSession.Current.Role == "Admin")
                                {
                                    UserSession.Current.FullName = "Quản trị viên";
                                }

                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoginAsync: {ex.Message}");
            }

            return false;
        }

        public void Logout()
        {
            UserSession.Current.Clear();
        }
    }
}