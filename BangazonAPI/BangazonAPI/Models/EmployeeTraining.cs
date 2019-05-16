using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class EmployeeTraining
    {
        public List<Employee> employee = new List<Employee>();
        public Employee getEmployeeById(int id)
        {
            return employee[id];
        }
        public List<TrainingProgram> trainingProgram = new List<TrainingProgram>();
        public TrainingProgram getTrainingProgramById(int id)
        {
            return trainingProgram[id];
        }
    }
}
