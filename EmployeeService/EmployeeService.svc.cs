using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EmployeeService.Models;
using Newtonsoft.Json;
using Npgsql;

namespace EmployeeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class EmployeeService : IEmployeeService
    {

        private const string connectionString = "Host=localhost;Database=test;Username=postgres;Password=123";
        public async Task<Employee> GetEmployeeById(int id)
        {
            var employees = await GetEmployeeReq(id);

            var employeeTree = BuildEmployeeTree(employees, id).FirstOrDefault();
            return employeeTree;
        }


       

        public async Task EnableEmployee(int id, int enable)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlUpdate = @"UPDATE ""Employee"" SET ""Enable"" = @enable WHERE ""Id"" = @employeeId";

                using (NpgsqlCommand updateCommand = new NpgsqlCommand(sqlUpdate, connection))
                {
                    Boolean enableAsBool = (enable == 1);

                    updateCommand.Parameters.AddWithValue("employeeId", id);
                    updateCommand.Parameters.Add("@enable", NpgsqlTypes.NpgsqlDbType.Bit).Value = enableAsBool;
                    int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Not Found: " + id);
                    }
                }
            }
        }



        private async Task<List<Employee>> GetEmployeeReq(int id)
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                List<Employee> employees = new List<Employee>();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    @"
                           WITH RECURSIVE RecursiveEmployee AS (
                            SELECT *
                            FROM ""Employee""
                            WHERE ""Id"" = @EmployeeId and ""Enable"" =  B'1'

                            UNION ALL

                            SELECT e.*
                            FROM ""Employee"" e
                            JOIN RecursiveEmployee r ON e.""EmployeeId"" = r.""Id""
	                        WHERE e.""Id"" !=  @EmployeeId and e.""Enable"" =  B'1'
                            )
                            SELECT *
                        FROM RecursiveEmployee;", connection))
                {
                    command.Parameters.AddWithValue("EmployeeId", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {


                            employees.Add(new Employee
                            {
                                Id = (int)reader["Id"],
                                Name = (string)reader["Name"],
                                ManagerId = reader["EmployeeId"] as int?,
                            });
                            Console.WriteLine($"EmployeeID: {reader["Id"]}, Name: {reader["Name"]}, ManagerID: {reader["EmployeeId"]}");
                        }
                    }
                }
                if (employees.Count == 0)
                {
                    throw new Exception("Not Found: " + id);
                }
                return employees;

            }
        }

        private List<Employee> BuildEmployeeTree(List<Employee> employees, int? managerId)
        {
            return employees
                .Where(e => e.ManagerId == managerId)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    Name = e.Name,
                    ManagerId = e.ManagerId,
                    Managers = BuildEmployeeTree(employees, e.Id)
                })
                .ToList();
        }


    }


}