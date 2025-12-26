using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ProjectMaui.Models;

namespace ProjectMaui.Services
{
    public class DepartmentService : BaseService
    {
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
    }
}
