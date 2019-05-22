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
    public class ProductTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypesController(IConfiguration config)
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

        //Gets All Product Types
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string productTypesColumns = @"
                        SELECT pt.id AS 'Product Type Id', 
                        pt.name AS 'Product Type Name'";
                    string productTypesTable = "FROM ProductType pt";

                    command = $"{productTypesColumns} {productTypesTable}";




                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> ProductTypes = new List<ProductType>();

                    while (reader.Read())
                    {

                        ProductType currentProductType = new ProductType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                            name = reader.GetString(reader.GetOrdinal("Product Type Name"))
                        };
                        ProductTypes.Add(currentProductType);
                    }
                    reader.Close();
                    return Ok(ProductTypes);
                }
            }
        }

        //Gets a Single Product Type
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string productTypesColumns = @"
                        SELECT pt.id AS 'Product Type Id', 
                        pt.name AS 'Product Type Name'";
                    string productTypesTable = "FROM ProductType pt WHERE pt.id = @id";

                    command = $"{productTypesColumns} {productTypesTable}";

                    cmd.CommandText = command;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType ProductType = null;

                    if (reader.Read())
                    {
                        ProductType = new ProductType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                            name = reader.GetString(reader.GetOrdinal("Product Type Name"))
                        };
                    }
                    reader.Close();

                    return Ok(ProductType);
                }
            }
        }

        //Creates a New Product Type
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType ProductType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (name) OUTPUT INSERTED.Id VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", ProductType.name));

                    int newId = (int)cmd.ExecuteScalar();
                    ProductType.id = newId;
                    return CreatedAtRoute("GetProduct", new { id = newId }, ProductType);
                }
            }
        }

        //Edits a Single Product Type
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType ProductType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType SET name=@n WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@n", ProductType.name));
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Deletes a Single Product
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
                        cmd.CommandText = @"DELETE FROM ProductType WHERE Id = @id";
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Determines if a Product exists
        private bool ProductTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT name FROM ProductType WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}