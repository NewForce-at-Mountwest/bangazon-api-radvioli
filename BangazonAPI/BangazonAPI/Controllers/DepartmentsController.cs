﻿using System;
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

        //GET request for Departments, allows inclusion of employees to query string and allows filtering by Budgets greater than $300,000
        [HttpGet]
        public async Task<IActionResult> Get(string include, string filter)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string departmentColumns = @"
                    SELECT d.Id AS 'Department Id',
                    d.Name AS 'Department Name',
                    d.Budget AS 'Department Budget'
                    ";

                    string departmentTables = @"
                    FROM Department d";

                    if (include == "employees")
                    {
                        string includeColumns = @",
                        e.Id AS 'Employee ID',
                        e.firstName AS 'Employee First Name',
                        e.lastName AS 'Eployee Last Name'
                        e.isSupervisor AS 'is Supervisor?'
                        e.DepartmentId AS 'Department ID'
                    ";

                        string includeTables = @"
                        JOIN Employees ON d.Id = e.DepartmentId
                     ";
                        command = $@"{departmentColumns}
                                     {includeColumns}
                                     {departmentTables}
                                     {includeTables}";

                    }
                    else
                    {
                        command = $"{departmentColumns} {departmentTables}";   

                    }
                    if (filter == "budget&_gt=300000")
                    {
                        command += $"WHERE d.Budget >= 300000";
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

                    if(reader.Read())
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
        //start here on resume

    }
}