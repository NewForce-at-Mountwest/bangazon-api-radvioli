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

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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

        //Gets All Computers
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string computersColumns = @"
                        SELECT c.Id AS 'Computer Id', 
                        c.PurchaseDate AS 'Computer Purchase Date', 
                        c.DecomissionDate AS 'Decomission Date (If Applicable)',
                        c.make AS 'Computer Make',
                        c.manufacturer AS 'Computer Manufacturer'";
                    string computersTable = "FROM Computer c";



                    command = $"{computersColumns} {computersTable}";




                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> Computers = new List<Computer>();

                    while (reader.Read())
                    {

                        Computer currentComputer = new Computer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("Computer Purchase Date")),
                            make = reader.GetString(reader.GetOrdinal("Computer Make")),
                            manufacturer = reader.GetString(reader.GetOrdinal("Computer Manufacturer"))

                        };

                        //Check to see if the decomission date is null.If not, add it to the object.  If it is, set the Decomission Date to null on the object.

                        if (!reader.IsDBNull(reader.GetOrdinal("Decomission Date (If Applicable)")))
                        {
                            currentComputer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("Decomission Date (If Applicable)"));
                        }
                        else
                        {
                            currentComputer.DecomissionDate = DateTime.MinValue;
                        }
                        Computers.Add(currentComputer);
                    }
                    reader.Close();
                    return Ok(Computers);
                }
            }
        }


        //Gets a Single Computer
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string computersColumns = @"
                        SELECT c.Id AS 'Computer Id', 
                        c.PurchaseDate AS 'Computer Purchase Date', 
                        c.DecomissionDate AS 'Decomission Date (If Applicable)',
                        c.make AS 'Computer Make',
                        c.manufacturer AS 'Computer Manufacturer'";
                    string computersTable = "FROM Computer c WHERE id = @id";

                    command = $"{computersColumns} {computersTable}";

                    cmd.CommandText = command;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer Computer = null;

                    if (reader.Read())
                    {
                        Computer = new Computer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("Computer Purchase Date")),
                            make = reader.GetString(reader.GetOrdinal("Computer Make")),
                            manufacturer = reader.GetString(reader.GetOrdinal("Computer Manufacturer"))
                        };
                        //Checking to see if Decomission Date is null

                        if (!reader.IsDBNull(reader.GetOrdinal("Decomission Date (If Applicable)")))
                        {
                            Computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("Decomission Date (If Applicable)"));
                        }
                        else
                        {
                            Computer.DecomissionDate = DateTime.MinValue;
                        }
                    }
                    reader.Close();

                    return Ok(Computer);
                }
            }
        }

        //Creates a New Computer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer Computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, DecomissionDate, make, manufacturer) OUTPUT INSERTED.Id VALUES (@pd, @dd, @make, @manu)";
                    cmd.Parameters.Add(new SqlParameter("@pd", Computer.PurchaseDate));
                    cmd.Parameters.Add(new SqlParameter("@dd", Computer.DecomissionDate));
                    cmd.Parameters.Add(new SqlParameter("@make", Computer.make));
                    cmd.Parameters.Add(new SqlParameter("@manu", Computer.manufacturer));

                    int newId = (int)cmd.ExecuteScalar();
                    Computer.id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, Computer);
                }
            }
        }

        //Edits a Single Computer
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer Computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer SET PurchaseDate=@pd, DecomissionDate=@dd, make=@make, manufacturer=@manu WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@pd", Computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@dd", Computer.DecomissionDate));
                        cmd.Parameters.Add(new SqlParameter("@make", Computer.make));
                        cmd.Parameters.Add(new SqlParameter("@manu", Computer.manufacturer));
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
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Deletes a Single Computer
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
                        cmd.CommandText = @"DELETE FROM Computer WHERE id = @id";
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
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Determines if a Computer exists
        private bool ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT PurchaseDate, DecomissionDate, make, manufacturer FROM Computer WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}