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
        public List<Department> department { get; set; } = new List<Department>();
        public Department getDepartmentById(int id)
        {
            return department[id];
        }
    }
}
