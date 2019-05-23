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
                        FROM [Order] o WHERE PaymentTypeId IS NULL
                        JOIN PaymentType pm ON o.PaymentTypeId = pm.id					
						
                        ";
                        command = $@"
                                    {completedColumns}
                                    
                                    {completedTables}";
                    }
                    if (include == "product")
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

                    if (include == "customer")
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
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("Payment Type Id")),
                            ordersPayment = new PaymentType()
                            {
                                name = reader.GetString(reader.GetOrdinal("Payment Type Name")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                            },

                        };
                        
                        if (include == "product")
                        {
                            Product currentProduct = new Product
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                                price = reader.GetInt32(reader.GetOrdinal("Product Price")),
                                title = reader.GetString(reader.GetOrdinal("Product Description")),
                                description = reader.GetString(reader.GetOrdinal("Product Description")),
                                quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity"))

                            };

                            //if (Orders.Any(o => o.id == currentOrder.id))
                            //{
                            //    Order thisOrder = Orders.Where(o => o.id == currentOrder.id).FirstOrDefault();
                            //    thisOrder.Products.Add(currentProduct);
                            //}
                            //else
                            //{
                                currentOrder.Products.Add(currentProduct);
                                Orders.Add(currentOrder);

                            
                        }
                        if (include == "customer")
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
    }
}
        
    
