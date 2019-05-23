using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

//employee controller
namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    //joining tables
                    string command = $@"SELECT e.Id AS 'Employee Id', 
                                               e.FirstName, 
                                               e.LastName, 
                                               e.IsSuperVisor, 
                                               e.DepartmentId,
                                               d.Id AS 'Department Id', 
                                               d.Name AS 'Department', 
                                               c.Id AS 'Computer Id',
                                               c.purchaseDate,
                                               c.make,
                                               c.manufacturer,
                                               c.decomissionDate
						
                        FROM Employee e LEFT JOIN Department d ON e.DepartmentId = d.Id
						LEFT JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                        LEFT JOIN Computer c ON ce.ComputerId= c.Id";

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Employee> employees = new List<Employee>();

                    while (reader.Read())
                    {

                      

                        Employee employee = new Employee
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            employeesDepartment = new Department()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                name = reader.GetString(reader.GetOrdinal("Department"))
                            },
                            isSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            employeeComputer = null

                        };
                        //assigning computer to employee id
                        if (!reader.IsDBNull(reader.GetOrdinal("Computer Id")))
                        {
                            Computer computer = new Computer()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                                make = reader.GetString(reader.GetOrdinal("make")),
                                manufacturer = reader.GetString(reader.GetOrdinal("manufacturer")),
                                datePurchased = reader.GetDateTime(reader.GetOrdinal("purchaseDate")),

                            };
                            employee.employeeComputer = computer;
                        }

                        //checking for decommission date

                        if (!reader.IsDBNull(reader.GetOrdinal("decomissionDate")))
                        {
                            employee.employeeComputer.dateDecommissioned = reader.GetDateTime(reader.GetOrdinal("decomissionDate"));

                        }


                        employees.Add(employee);
                    }
                    reader.Close();
                    return Ok(employees);
                }
            }
        }




        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {

                //getting information for single person
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS 'Employee Id', 
                                               e.FirstName, 
                                               e.LastName, 
                                               e.IsSuperVisor, 
                                               e.DepartmentId,
                                               d.Id AS 'Department Id', 
                                               d.Name AS 'Department',
                                               c.Id AS 'Computer Id',
                                            c.purchaseDate,
                                               c.make,
                                               c.manufacturer,
                                               c.decomissionDate
                        FROM Employee e LEFT JOIN Department d ON e.DepartmentId = d.Id
						LEFT JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                        LEFT JOIN Computer c ON ce.ComputerId= c.Id WHERE e.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read())
                    {

                        employee = new Employee
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            employeesDepartment = new Department()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                name = reader.GetString(reader.GetOrdinal("Department"))
                            },
                            isSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            employeeComputer = null

                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("Computer Id")))
                        {
                            Computer computer = new Computer()
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                                make = reader.GetString(reader.GetOrdinal("make")),
                                manufacturer = reader.GetString(reader.GetOrdinal("manufacturer")),
                                datePurchased = reader.GetDateTime(reader.GetOrdinal("purchaseDate")),

                            };
                            employee.employeeComputer = computer;
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("decomissionDate")))
                        {
                            employee.employeeComputer.dateDecommissioned = reader.GetDateTime(reader.GetOrdinal("decomissionDate"));

                        }


                    }
                    reader.Close();

                    return Ok(employee);
                }
            }
        }

        [HttpPost]

        //post new employee
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, IsSuperVisor, DepartmentId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @IsSuperVisor, @DepartmentId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.firstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.lastName));
                    cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.isSupervisor));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));

                    int newId = (int)cmd.ExecuteScalar();
                    employee.id = newId;
                    return CreatedAtRoute("getEmployee", new { id = newId }, employee);
                }
            }
        }

        [HttpPut("{id}")]

        //update an employee
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText =
                            @"UPDATE Employee 
                            SET FirstName = @FirstName,
                            LastName = @LastName,
                            IsSuperVisor = @IsSuperVisor,
                            DepartmentId = @DepartmentId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.firstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.lastName));
                        cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.isSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
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
                if (!EmployeeExists(id))
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

        //delete an employee
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                       
                            cmd.CommandText = @"DELETE Employee Where Id = @id";


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
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool EmployeeExists(int id)

        //checking for existing employee
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, IsSuperVisor, DepartmentId
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}