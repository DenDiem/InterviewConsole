using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmployeeService;
namespace InterviewConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var employeeService = new EmployeeService.EmployeeService();
            var a = await employeeService.GetEmployeeById(1);
            await employeeService.EnableEmployee(1, 0);


            // DataTable dtEmployees = GetQueryResult("SELECT * FROM Employee");
        }
        
        private static DataTable GetQueryResult(string query)
        {
            var dt = new DataTable();

			using (var connection = new SqlConnection("Data Source=(local);Initial Catalog=Test;User ID=sa;Password=pass@word1; "))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
					command.CommandText = query;

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

			return dt;
        }
    }
}
