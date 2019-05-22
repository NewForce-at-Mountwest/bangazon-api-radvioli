using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomersController(IConfiguration config)
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


        //GET request for all Customers, allows inclusion of Products and PaymentTypes to query string, and allows query string pattern matching against firstName and lastName values in the databse 
        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    string command = "";
                    //creates GET query for Customer
                    string customerColumns = @"
                        SELECT c.Id AS 'Customer Id', 
                        c.FirstName AS 'Customer First Name', 
                        c.LastName AS 'Customer Last Name',
                       c.lastActive AS 'Last Date Active',
                        c.accountCreated AS 'Account Created Date'";

                    string customerTable = "FROM Customer c";

                    //Adds query string for Product and ProductType fields if added
                    if (include == "product")
                    {
                        string includeColumns = @", 
                        p.Id AS 'Product ID', 
						p.ProductTypeId AS 'Product Type ID',
						p.CustomerId AS 'Customer ID',
                        p.Price AS 'Product Price', 
                        p.Title AS 'Product Title',
						p.Description AS 'Product Description',
						p.Quantity AS 'Product Quantity',
						pt.Id AS 'Product Type Id',
						pt.Name AS 'Product Type Name'						
						";

                        //Joins Customers, Products and ProductTypes 
                        string includeTables = @"
                        JOIN Product p ON c.Id = p.CustomerId 
                        JOIN ProductType pt ON p.ProductTypeId=pt.Id";

                        command = $@"{customerColumns} 
                                    {includeColumns} 
                                    {customerTable} 
                                    {includeTables}";
                    }
                    //Adds query string for PaymentType fields if added
                    else if (include == "payment")
                    {
                        string includeColumns2 = @",
						pm.Id AS 'Payment ID',
						pm.AcctNumber AS 'Payment Account Number',
						pm.Name AS 'Pament Name',
						pm.CustomerID AS 'Customer ID'
						";

                        //Joins Customers and PaymentTypes
                        string includeTables2 = @"
						JOIN PaymentType pm ON c.Id = pm.CustomerID";

                        //Concatinates aforementioned query strings together if Pruducts and Payments are included
                        command = $@"{customerColumns}
                                     {includeColumns2}
                                     {customerTable}
                                     {includeTables2}";

                    }
                    else
                    {
                        //Query string requesting only Customers 
                        command = $"{customerColumns} {customerTable}";
                    }

                    if (q != null)
                    {
                        //Allows for pattern matching-ish queries against Customer.FirstName OR Customer.LastName
                        command += $" WHERE c.FirstName LIKE '{q}%' OR c.LastName LIKE '{q}%'";
                    }
                    
                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {

                        Customer currentCustomer = new Customer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            firstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                            lastName = reader.GetString(reader.GetOrdinal("Customer Last Name")),
                            accountCreated = reader.GetDateTime(reader.GetOrdinal("Account Created Date")),
                            lastActive = reader.GetDateTime(reader.GetOrdinal("Last Date Active"))
                        };

                        if (include == "product")
                        {
                            Product currentProduct = new Product
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Product ID")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type ID")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Customer ID")),
                                price = reader.GetDecimal(reader.GetOrdinal("Product Price")),
                                title = reader.GetString(reader.GetOrdinal("Product Title")),
                                description = reader.GetString(reader.GetOrdinal("Product Description")),
                                quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity")),
                            };

                            // If the CUstomer is already on the list, don't add them again!
                            if (customers.Any(c => c.id == currentCustomer.id))
                            {
                                Customer thisCustomer = customers.Where(c => c.id == currentCustomer.id).FirstOrDefault();
                                thisCustomer.Products.Add(currentProduct);
                            }
                            else
                            {
                                currentCustomer.Products.Add(currentProduct);
                                customers.Add(currentCustomer);

                            }

                        }

                        else if (include == "payment")

                        {
                            PaymentType currentPayment = new PaymentType
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Payment ID")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                                name = reader.GetString(reader.GetOrdinal("Pament Name"))
                            };


                            // If the CUstomer is already on the list, don't add them again!
                            if (customers.Any(c => c.id == currentCustomer.id))
                            {
                                Customer thisCustomer = customers.Where(c => c.id == currentCustomer.id).FirstOrDefault();
                                thisCustomer.PaymentTypes.Add(currentPayment);
                            }
                            else
                            {
                                currentCustomer.PaymentTypes.Add(currentPayment);
                                customers.Add(currentCustomer);

                            };

                        }


                        else
                        {
                            customers.Add(currentCustomer);
                        }


                    }

                    reader.Close();
                    return Ok(customers);


                }
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                SELECT
                                    Id, FirstName, LastName, AccountCreated, LastActive
                                FROM Customer
                                WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer Customer = null;

                    if (reader.Read())
                    {
                        Customer = new Customer
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            firstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            lastName = reader.GetString(reader.GetOrdinal("LastName")),
                            accountCreated = reader.GetDateTime(reader.GetOrdinal("AccountCreated")),
                            lastActive = reader.GetDateTime(reader.GetOrdinal("LastActive"))
                        };
                    }
                    reader.Close();

                    return Ok(Customer);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer Customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (firstName, lastName, accountCreated, lastActive)
                                                OUTPUT INSERTED.Id
                                                VALUES (@firstName, @lastName, @accountCreated, @lastActive)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", Customer.firstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", Customer.lastName));
                    cmd.Parameters.Add(new SqlParameter("@accountCreated", Customer.accountCreated));
                    cmd.Parameters.Add(new SqlParameter("@lastActive", Customer.lastActive));



                    int newId = (int)cmd.ExecuteScalar();
                    Customer.id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, Customer);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer Customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                                    SET firstName=@firstName, 
                                                    lastName=@lastName, 
                                                    accountCreated=@accountCreated,
                                                    lastActive=@lastActive
                                                    WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", Customer.firstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", Customer.lastName));
                        cmd.Parameters.Add(new SqlParameter("@accountCreated", Customer.accountCreated));
                        cmd.Parameters.Add(new SqlParameter("@lastActive", Customer.lastActive));
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
                if (!CustomerExists(id))
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
                        cmd.CommandText = @"DELETE FROM Customer WHERE Id = @id";
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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                SELECT
                                    Id, firstName, lastName, accountCreated, lastActive
                                FROM Customer
                                WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }

            }
        }
    }
}