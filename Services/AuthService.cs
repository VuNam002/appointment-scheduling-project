using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ProjectMaui.Services
{
    // 1. Thêm kế thừa : BaseService
    public class AuthService : BaseService
    {
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

        public async Task<string> RegisterDoctorAsync(string doctorName, string phone, string email, string password, int departmentId, string specialization)
        {
            if (UserSession.Current.Role != "Admin")
            {
                return "Bạn không có quyền thực hiện chức năng này.";
            }

            int doctorRoleId = 2; // Assuming 'Doctor' role has ID 2

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check for existing account by phone number
                string checkAccountQuery = "SELECT COUNT(1) FROM Accounts WHERE PhoneNumber = @PhoneNumber";
                using (SqlCommand checkAccountCommand = new SqlCommand(checkAccountQuery, connection))
                {
                    checkAccountCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                    if ((int)await checkAccountCommand.ExecuteScalarAsync() > 0)
                    {
                        return "Số điện thoại đã được sử dụng cho tài khoản khác.";
                    }
                }

                // Check for existing doctor by email
                string checkDoctorQuery = "SELECT COUNT(1) FROM Doctors WHERE Email = @Email";
                using (SqlCommand checkDoctorCommand = new SqlCommand(checkDoctorQuery, connection))
                {
                    checkDoctorCommand.Parameters.AddWithValue("@Email", email);
                    if ((int)await checkDoctorCommand.ExecuteScalarAsync() > 0)
                    {
                        return "Email đã được sử dụng cho bác sĩ khác.";
                    }
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Create Account (without Email)
                        string accountQuery = @"
                            INSERT INTO Accounts (PhoneNumber, PasswordHash, RoleId, IsActive, CreatedAt)
                            OUTPUT INSERTED.AccountId
                            VALUES (@PhoneNumber, @PasswordHash, @RoleId, 1, GETDATE());";

                        int accountId;
                        using (SqlCommand accountCommand = new SqlCommand(accountQuery, connection, transaction))
                        {
                            accountCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                            accountCommand.Parameters.AddWithValue("@PasswordHash", password);
                            accountCommand.Parameters.AddWithValue("@RoleId", doctorRoleId);
                            
                            accountId = (int)await accountCommand.ExecuteScalarAsync();
                        }

                        if (accountId <= 0)
                        {
                            throw new Exception("Account creation failed, returned no AccountId.");
                        }

                        // 2. Create Doctor (with Email)
                        string doctorQuery = @"
                            INSERT INTO Doctors (DoctorName, Phone, Email, DepartmentId, Specialization, AccountId)
                            VALUES (@DoctorName, @Phone, @Email, @DepartmentId, @Specialization, @AccountId);";
                        
                        using (SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection, transaction))
                        {
                            doctorCommand.Parameters.AddWithValue("@DoctorName", doctorName);
                            doctorCommand.Parameters.AddWithValue("@Phone", phone);
                            doctorCommand.Parameters.AddWithValue("@Email", email);
                            doctorCommand.Parameters.AddWithValue("@DepartmentId", departmentId);
                            doctorCommand.Parameters.AddWithValue("@Specialization", specialization ?? (object)DBNull.Value);
                            doctorCommand.Parameters.AddWithValue("@AccountId", accountId);

                            await doctorCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return null; // Success
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi RegisterDoctorAsync: {ex.Message}");
                        await transaction.RollbackAsync();
                        return "Đã xảy ra lỗi trong quá trình đăng ký.";
                    }
                }
            }
        }
    }
}