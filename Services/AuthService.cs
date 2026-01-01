﻿using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Storage;

namespace ProjectMaui.Services
{
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
                          AND a.IsActive = 1
                          AND a.IsDeleted = 0";

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
                                await SecureStorage.SetAsync("Auth_Phone", phone);
                                await SecureStorage.SetAsync("Auth_Password", password);
                                await SecureStorage.SetAsync("Auth_LoginTime", DateTime.UtcNow.ToString()); // Dùng UTC

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
        public async Task<string> SoftDeleteUserAsync(int targetAccountId)
        {
            if(UserSession.Current.Role != "Admin") 
            { 
                return "Bạn không có quyền thực hiện chức năng này.";
            }
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // BƯỚC 1: Xóa mềm trong bảng ACCOUNTS
                        string queryAccount = @"
                        UPDATE Accounts 
                        SET IsDeleted = 1, 
                        DeletedAt = GETDATE(), 
                        IsActive = 0 
                        WHERE AccountId = @AccountId";

                        using (SqlCommand cmdAccount = new SqlCommand(queryAccount, connection, transaction))
                        {
                            cmdAccount.Parameters.AddWithValue("@AccountId", targetAccountId);
                            int rowsAffected = await cmdAccount.ExecuteNonQueryAsync();

                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return "Không tìm thấy tài khoản cần xóa.";
                            }
                        }

                        // BƯỚC 2: Xóa mềm trong bảng DOCTORS (Nếu tài khoản này là Bác sĩ)
                        string queryDoctor = @"
                            UPDATE Doctors 
                            SET IsDeleted = 1, 
                            DeletedAt = GETDATE() 
                            WHERE AccountId = @AccountId";

                        using (SqlCommand cmdDoctor = new SqlCommand(queryDoctor, connection, transaction))
                        {
                            cmdDoctor.Parameters.AddWithValue("@AccountId", targetAccountId);
                            await cmdDoctor.ExecuteNonQueryAsync();
                        }

                        // BƯỚC 3: Xóa mềm trong bảng PATIENTS (Nếu tài khoản này là Bệnh nhân)
                        string queryPatient = @"
                            UPDATE Patients 
                            SET IsDeleted = 1, 
                            DeletedAt = GETDATE() 
                            WHERE AccountId = @AccountId";

                        using (SqlCommand cmdPatient = new SqlCommand(queryPatient, connection, transaction))
                        {
                            cmdPatient.Parameters.AddWithValue("@AccountId", targetAccountId);
                            await cmdPatient.ExecuteNonQueryAsync();
                        }

                        // Nếu mọi thứ OK thì Commit
                        transaction.Commit();
                        return null; // Trả về null nghĩa là thành công
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Hoàn tác nếu có lỗi
                        System.Diagnostics.Debug.WriteLine($"Lỗi SoftDeleteUserAsync: {ex.Message}");
                        return "Đã xảy ra lỗi khi xóa tài khoản.";
                    }
                }
            }
        }

        public void Logout()
        {
            UserSession.Current.Clear();
            SecureStorage.Remove("Auth_Phone");
            SecureStorage.Remove("Auth_Password");
            SecureStorage.Remove("Auth_LoginTime");
        }

        public async Task<(string ErrorMessage, int NewDoctorId)> RegisterDoctorAsync(string doctorName, string phone, string password, int departmentId, string specialization)
        {
            if (UserSession.Current.Role != "Admin")
            {
                return ("Bạn không có quyền thực hiện chức năng này.", 0);
            }

            int doctorRoleId = 2; 

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string checkUserQuery = "SELECT COUNT(1) FROM Accounts WHERE PhoneNumber = @PhoneNumber";
                using (SqlCommand checkUserCommand = new SqlCommand(checkUserQuery, connection))
                {
                    checkUserCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                    int userExists = (int)await checkUserCommand.ExecuteScalarAsync();
                    if (userExists > 0)
                    {
                        return ("Số điện thoại đã tồn tại.", 0);
                    }
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
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

                        // 2. Create Doctor
                        string doctorQuery = @"
                            INSERT INTO Doctors (DoctorName, Phone, DepartmentId, Specialization, AccountId)
                            OUTPUT INSERTED.DoctorId
                            VALUES (@DoctorName, @Phone, @DepartmentId, @Specialization, @AccountId);";
                        
                        int newDoctorId;
                        using (SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection, transaction))
                        {
                            doctorCommand.Parameters.AddWithValue("@DoctorName", doctorName);
                            doctorCommand.Parameters.AddWithValue("@Phone", phone);
                            doctorCommand.Parameters.AddWithValue("@DepartmentId", departmentId);
                            doctorCommand.Parameters.AddWithValue("@Specialization", specialization ?? (object)DBNull.Value);
                            doctorCommand.Parameters.AddWithValue("@AccountId", accountId);

                            newDoctorId = (int)await doctorCommand.ExecuteScalarAsync();
                        }

                        transaction.Commit();
                        return (null, newDoctorId); // Success
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi RegisterDoctorAsync: {ex.Message}");
                        await transaction.RollbackAsync();
                        return ("Đã xảy ra lỗi trong quá trình đăng ký.", 0);
                    }
                }
            }
        }

        public async Task<bool> TryAutoLoginAsync()
        {
            var phone = await SecureStorage.GetAsync("Auth_Phone");
            var password = await SecureStorage.GetAsync("Auth_Password");
            var loginTimeStr = await SecureStorage.GetAsync("Auth_LoginTime");

            if (!string.IsNullOrEmpty(phone) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(loginTimeStr))
            {
                if (DateTime.TryParse(loginTimeStr, out DateTime loginTime))
                {
                    // Nếu quá 30 phút kể từ lần cuối đăng nhập -> Hủy phiên
                    if (DateTime.Now - loginTime > TimeSpan.FromMinutes(30))
                    {
                        Logout(); 
                        return false;
                    }
                }
                return await LoginAsync(phone, password);
            }
            return false;
        }
    }
}