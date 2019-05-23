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
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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
        //GET request for all Orders, allows inclusion of Products and Customers and completed orders to query string.
        [HttpGet]
        public async Task<IActionResult> Get(string include, string completed)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string ordersColumns = @"
                        SELECT o.id AS 'Order Id', 
                        o.CustomerId AS 'Customer Id',                       
                        pm.id AS 'Payment Type Id',
                        pm.name AS 'Payment Type Name',
                        pm.AcctNumber AS 'Payment Account Number'						
                        ";

                    string ordersTable = @"
                        FROM [Order] o
                        JOIN PaymentType pm ON o.PaymentTypeId = pm.id											
                        ";

                    if (completed == "false")
                    {
                        string completedColumns = @"
                        SELECT o.id AS 'Order Id', 
                        o.CustomerId AS 'Customer Id',                       
                        pm.id AS 'Payment Type Id',
                        pm.name AS 'Payment Type Name',
                        pm.AcctNumber AS 'Payment Account Number'
						
                        ";

                        string completedTables = @"
                        FROM [Order] o 
                        LEFT JOIN PaymentType pm ON o.PaymentTypeId = pm.id					
						WHERE PaymentTypeId IS NULL
                        ";
                        command = $@"
                                    {completedColumns}
                                    
                                    {completedTables}";
                    }
                    else if (include == "product")
                    {
                        string includePColumns = @", 
                        op.Productid AS 'Product Id',
                        p.productTypeId AS 'Product Type Id',                                           
                        p.price AS 'Product Price',
                        p.title AS 'Product Title',
                        p.description AS 'Product Description',
                        p.quantity AS 'Product Quantity'
                        
                        ";
                        string includePTables = @"
                        
                        JOIN OrderProduct op ON o.id = op.OrderId
						JOIN Product p ON op.ProductId = p.Id
                            ";

                        command = $@"{ordersColumns}
                                    {includePColumns}
                                     {ordersTable}
                                    {includePTables}";
                    }

                    else if (include == "customer")
                    {
                        string includeColumns = @", 
                        c.FirstName AS 'Customer First Name',
                        c.LastName AS 'Customer Last Name',
                        c.accountCreated AS 'Account Creation Date',
                        c.lastActive AS 'Last User Login Date'
                        
                        ";
                        string includeTables = @"
                        JOIN Customer c ON o.CustomerId = c.id
                        
                        
                        ";

                        command = $@"{ordersColumns}
                                    {includeColumns}
                                     {ordersTable}
                                    {includeTables}";
                    }
                    else
                    {
                        command = $"{ordersColumns} {ordersTable}";
                    }
                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> Orders = new List<Order>();

                    while (reader.Read())
                    {
                        Order currentOrder = new Order
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Order Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("Payment Type Id")))
                        {
                            currentOrder.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("Payment Type Id"));
                            currentOrder.ordersPayment = new PaymentType()
                            {
                                name = reader.GetString(reader.GetOrdinal("Payment Type Name")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                            };
                        }
                        if (include == "product")
                        {
                            Product currentProduct = new Product
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                                price = reader.GetDecimal(reader.GetOrdinal("Product Price")),
                                title = reader.GetString(reader.GetOrdinal("Product Description")),
                                description = reader.GetString(reader.GetOrdinal("Product Description")),
                                quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity"))
                            };
                            currentOrder.Products.Add(currentProduct);
                            Orders.Add(currentOrder);
                        }
                        else if (include == "customer")
                        {
                            Customer currentCustomer = new Customer
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                                firstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                                lastName = reader.GetString(reader.GetOrdinal("Customer Last Name")),
                                accountCreated = reader.GetDateTime(reader.GetOrdinal("Account Creation Date")),
                                lastActive = reader.GetDateTime(reader.GetOrdinal("Last User Login Date")),
                            };
                            currentOrder.ordersCustomer = currentCustomer;
                            Orders.Add(currentOrder);
                        }
                        else
                        {
                            Orders.Add(currentOrder);
                        }
                    }
                    reader.Close();
                    return Ok(Orders);
                }
            }
        }
        //Get a single order
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = "";

                    string ordersColumns = @"
                        SELECT o.id AS 'Order Id', 
                        o.CustomerId AS 'Customer Id',                       
                        pm.id AS 'Payment Type Id',
                        pm.name AS 'Payment Type Name',
                        pm.AcctNumber AS 'Payment Account Number'						
                        ";

                    string ordersTable = @"
                        FROM [Order] o 
                        JOIN PaymentType pm ON o.PaymentTypeId = pm.id	
                        WHERE o.id = @id
                        ";
                    command = $"{ordersColumns} {ordersTable}";
                
                cmd.CommandText = command;
                cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order Order = null;

                    if (reader.Read())
                    {
                        Order = new Order
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Order Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("Payment Type Id")))
                        {
                            Order.PaymentTypeId = reader.GetInt32(reader.GetOrdinal("Payment Type Id"));
                            Order.ordersPayment = new PaymentType()
                            {
                                name = reader.GetString(reader.GetOrdinal("Payment Type Name")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                            };
                        }
                    };

                    reader.Close();

                    return Ok(Order);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order Order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (PaymentTypeId, CustomerId) 
                                        OUTPUT INSERTED.id VALUES (@ptd, @cd)";
                    cmd.Parameters.Add(new SqlParameter("@ptd", Order.PaymentTypeId));
                    cmd.Parameters.Add(new SqlParameter("@cd", Order.CustomerId));
                    

                    int newId = (int)cmd.ExecuteScalar();
                    Order.id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, Order);
                }
            }
        }
        //Edits a Single Order
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order Order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order] SET PaymentTypeId=@ptd, CustomerId=@cd WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@ptd", Order.PaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@cd", Order.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@id", Order.id));

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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        //This method deletes a single Order
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
                        cmd.CommandText = @"DELETE FROM [Order] WHERE id = @id";
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        // This method checks to see if an order exists. Functions in the delete and edit functionality
        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT PaymentTypeId, CustomerId FROM [Order] WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}



   
        
    
