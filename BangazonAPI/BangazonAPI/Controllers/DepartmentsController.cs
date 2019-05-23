using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //GET request for Departments, allows inclusion of employees to query string and allows filtering by Budgets greater than $30,000
        [HttpGet]
        public async Task<IActionResult> Get(string include, string filter, string gt)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    //creates GET query for Department
                    string departmentColumns = @"
                    SELECT d.Id AS 'Department Id',
                    d.Name AS 'Department Name',
                    d.Budget AS 'Department Budget'
                    ";

                    string departmentTables = @"
                    FROM Department d";
                    //Adds query string for employees if added
                    if (include == "employees")
                    {

                        string includeColumns = @", e.id AS 'Employee ID',
                        e.firstName AS 'Employee First Name',
                        e.lastName AS 'Employee Last Name',
                        e.isSupervisor AS 'is Supervisor?',
                        e.DepartmentId AS 'Department ID'
                    ";
                        //Joins Departments with employees
                        string includeTables = @"
                        JOIN Employee e ON d.Id = e.DepartmentId
                     ";
                        command = $@"{departmentColumns}
                                     {includeColumns}
                                     {departmentTables}
                                     {includeTables}";

                    }
                    
                    //Adds a filter query string that returns departments with budget greater than 30000
                    else if (filter == "budget" && gt == "30000")
                    {
                        command = $"{departmentColumns}{departmentTables} WHERE d.Budget >= 30000";
                    }
                    

                    else
                    {
                        command = $"{departmentColumns} {departmentTables}";

                    }
                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();

                    while (reader.Read())
                    {
                        Department currentDepartment = new Department
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Department ID")),
                            name = reader.GetString(reader.GetOrdinal("Department Name")),
                            budget = reader.GetInt32(reader.GetOrdinal("Department Budget"))
                        };

                        if (include == "employees")
                        {
                            Employee currentEmployee = new Employee
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Employee ID")),
                                firstName = reader.GetString(reader.GetOrdinal("Employee First Name")),
                                lastName = reader.GetString(reader.GetOrdinal("Employee Last Name")),
                                isSupervisor = reader.GetBoolean(reader.GetOrdinal("is Supervisor?")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("Department ID"))
                            };
                            //If department already exists, don't add them again!
                            if (departments.Any(d => d.id == currentDepartment.id))
                            {
                                Department thisDepartment = departments.Where(d => d.id == currentDepartment.id).FirstOrDefault();
                                thisDepartment.employees.Add(currentEmployee);
                            }
                            else
                            {
                                currentDepartment.employees.Add(currentEmployee);
                                departments.Add(currentDepartment);
                            }
                        }
                        else
                        {
                            departments.Add(currentDepartment);
                        }
                    }

                    reader.Close();
                    return Ok(departments);
                }
            }
        }


        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                SELECT
                                    Id, Name, Budget
                                FROM Department
                                WHERE Id=@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Department Department = null;

                    if (reader.Read())
                    {
                        Department = new Department
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            name = reader.GetString(reader.GetOrdinal("Name")),
                            budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                        };
                    }
                    reader.Close();

                    return Ok(Department);
                }

            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department Department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Name, @Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", Department.name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", Department.budget));

                    int newId = (int)cmd.ExecuteScalar();
                    Department.id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, Department);
                }
            }

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department Department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                            SET Name=@Name, Budget=@Budget
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", Department.name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", Department.budget));
                        cmd.Parameters.Add(new SqlParameter("@id", id));


                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }

            }
            catch (Exception)
            {
                if (!DepartmentExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Department
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }

            }
            catch (Exception)
            {
                if (!DepartmentExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool DepartmentExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                SELECT Id, Name, Budget
                                FROM Department
                                WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }

            }
        }
    }
}