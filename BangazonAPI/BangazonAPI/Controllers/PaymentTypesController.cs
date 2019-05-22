using BangazonAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace BangazonAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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


        //GET request
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT Id, AcctNumber, [Name], CustomerId FROM PaymentType";



                    cmd.CommandText = commandText;



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    PaymentType paymentType = null;


                    while (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("Id")),
                            accountNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            name = reader.GetString(reader.GetOrdinal("Name")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        };



                        paymentTypes.Add(paymentType);
                    }


                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }



        //GET single payment type
        [HttpGet("{id}", Name = "PaymentType")]
        public async Task<IActionResult> GetSinglePaymentType([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Id, AcctNumber, [Name], CustomerId from PaymentType WHERE Id=@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType paymentTypeToDisplay = null;

                    while (reader.Read())
                    {

                        {
                            paymentTypeToDisplay = new PaymentType
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                accountNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                            };
                        };
                    };


                    reader.Close();

                    return Ok(paymentTypeToDisplay);
                }
            }
        }

        //  POST payment type
        [HttpPost]
        public async Task<IActionResult> PostPaymentType([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = $@"INSERT INTO PaymentType (AcctNumber, [Name], CustomerId)
                                                    OUTPUT INSERTED.Id
                                                    VALUES (@AcctNumber, @Name, @CustomerId)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.accountNumber));
                    cmd.Parameters.Add(new SqlParameter("@Name", paymentType.name));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));




                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.id = newId;
                    return CreatedAtRoute("PaymentType", new { id = newId }, paymentType);
                }
            }
        }

        // PUT edit payment type
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentType([FromRoute] int id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                                        SET AcctNumber = @AcctNumber,
                                                        Name = @Name,
                                                        CustomerId=@CustomerId
                                                        WHERE id = @id";

                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.accountNumber));
                        cmd.Parameters.Add(new SqlParameter("@Name", paymentType.name));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE a payment type
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentType([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                            cmd.CommandText = @"DELETE PaymentType
                                              WHERE id = @id";
                        
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, name
                                    FROM PaymentType
                                    WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}
