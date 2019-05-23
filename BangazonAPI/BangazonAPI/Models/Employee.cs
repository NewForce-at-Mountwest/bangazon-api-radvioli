using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isSupervisor { get; set; }
        public int DepartmentId { get; set; }
        public int ComputerId { get; set; }
        public Department employeesDepartment { get; set; } = new Department();

        public Computer employeeComputer { get; set; } = new Computer();
    }
}
