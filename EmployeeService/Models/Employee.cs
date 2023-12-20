using System.Collections.Generic;

namespace EmployeeService.Models
{

    public class Employee
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Employee> Managers { get; set; }
        public int? ManagerId { get; internal set; }
    }

}
