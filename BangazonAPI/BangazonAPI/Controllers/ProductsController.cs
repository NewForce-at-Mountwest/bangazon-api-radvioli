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
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductsController(IConfiguration config)
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

        //Gets All Products including Product Type and Customer
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";

                    string productsColumns = @"
                        SELECT p.Id AS 'Product Id', 
                        p.price AS 'Product Price', 
                        p.title AS 'Product Title',
                        p.description AS 'Product Description',
                        p.quantity AS 'Product Quantity', 
                        pt.Id AS 'Product Type Id',
                        pt.name AS 'Product Type Name',
                        c.Id AS 'Customer Id',
                        c.firstName AS 'Customer First Name',
                        c.lastName AS 'Customer Last Name',
                        c.accountCreated AS 'Account Creation Date',
                        c.lastActive AS 'Last User Login Date'";
                    string productsTable = "FROM Product p JOIN ProductType pt ON p.ProductTypeId = pt.Id JOIN Customer c on p.CustomerId = c.Id";

                    command = $"{productsColumns} {productsTable}";




                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Product> Products = new List<Product>();

                    while (reader.Read())
                    {

                        Product currentProduct = new Product
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            price = reader.GetDecimal(reader.GetOrdinal("Product Price")),
                            title = reader.GetString(reader.GetOrdinal("Product Title")),
                            description = reader.GetString(reader.GetOrdinal("Product Description")),
                            quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id"))
                        };
                        Products.Add(currentProduct);
                    }
                    reader.Close();
                    return Ok(Products);
                }
            }
        }

        //Gets a Single Product Including Product Type and Customer
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string productsColumns = @"
                        SELECT p.Id AS 'Product Id', 
                        p.price AS 'Product Price', 
                        p.title AS 'Product Title',
                        p.description AS 'Product Description',
                        p.quantity AS 'Product Quantity', 
                        pt.Id AS 'Product Type Id',
                        pt.name AS 'Product Type Name',
                        c.Id AS 'Customer Id',
                        c.firstName AS 'Customer First Name',
                        c.lastName AS 'Customer Last Name',
                        c.accountCreated AS 'Account Creation Date',
                        c.lastActive AS 'Last User Login Date'";
                    string productsTable = "FROM Product p " +
                        "JOIN ProductType pt ON p.ProductTypeId = pt.Id " +
                        "JOIN Customer c on p.CustomerId = c.Id " +
                        "WHERE p.Id = @id";

                    command = $"{productsColumns} {productsTable}";

                    cmd.CommandText = command;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Product Product = null;

                    if (reader.Read())
                    {
                        Product = new Product
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                            price = reader.GetDecimal(reader.GetOrdinal("Product Price")),
                            title = reader.GetString(reader.GetOrdinal("Product Title")),
                            description = reader.GetString(reader.GetOrdinal("Product Description")),
                            quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id"))
                        };
                    }
                    reader.Close();

                    return Ok(Product);
                }
            }
        }

        //Creates a New Product
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product Product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Product (price, title, description, quantity, ProductTypeId, CustomerId) OUTPUT INSERTED.Id VALUES (@price, @title, @description, @quantity, @producttypeid, @customerid)";
                    cmd.Parameters.Add(new SqlParameter("@price", Product.price));
                    cmd.Parameters.Add(new SqlParameter("@title", Product.title));
                    cmd.Parameters.Add(new SqlParameter("@description", Product.description));
                    cmd.Parameters.Add(new SqlParameter("@quantity", Product.quantity));
                    cmd.Parameters.Add(new SqlParameter("@producttypeid", Product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@customerid", Product.CustomerId));


                    int newId = (int)cmd.ExecuteScalar();
                    Product.id = newId;
                    return CreatedAtRoute("GetProduct", new { id = newId }, Product);
                }
            }
        }

        //Edits a Single Product
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Product Product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product SET price=@p, title=@t, description=@d, quantity=@q, ProductTypeId=@pt, CustomerId=@c WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@p", Product.price));
                        cmd.Parameters.Add(new SqlParameter("@t", Product.title));
                        cmd.Parameters.Add(new SqlParameter("@d", Product.description));
                        cmd.Parameters.Add(new SqlParameter("@q", Product.quantity));
                        cmd.Parameters.Add(new SqlParameter("@pt", Product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@c", Product.CustomerId));
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
                if (!ProductExists(id))
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
                        cmd.CommandText = @"DELETE FROM Product WHERE Id = @id";
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
                if (!ProductExists(id))
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
        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            price, title, description, quantity
                        FROM Product
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}