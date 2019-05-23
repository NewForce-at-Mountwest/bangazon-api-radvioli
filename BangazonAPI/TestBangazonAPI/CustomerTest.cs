using Newtonsoft.Json;
using System;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Data.SqlClient;




namespace TestBangazonAPI
{

    public class CustomerTest
    {
        //Create a new Customer in the db and check for 200 OK status code
        public async Task<Customer> buildTestCustomer(HttpClient client)
        {
            Customer TestCustomer = new Customer
            {
                firstName = "Pete",
                lastName = "Rock",
                accountCreated = DateTime.Now,
                lastActive = DateTime.Now

            };

            string TestCustomerAsJSON = JsonConvert.SerializeObject(TestCustomer);

            HttpResponseMessage response = await client.PostAsync("api/customers", new StringContent(TestCustomerAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Customer newTestCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newTestCustomer;

        }

        //Delete the Customer you just created and make sure you get no content status code back

        public async Task DeleteTestCustomer(Customer TestCustomer, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/customers/{TestCustomer.id}");

            deleteResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        //This TEST gets all Customers from the database
        [Fact]
        public async Task Test_Get_ALL_Customers()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage response = await client.GetAsync("api/customers");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                List<Customer> customerList = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.True(customerList.Count > 0);
            }
        }

        //This test creates a new customer, performs a get for single customer just created based on customer Id and validates data matches on firstName and LastName fields then deletes the newly created customer
        [Fact]

        public async Task Test_Get_Single_Customer()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a new TestCustomer
                Customer newTestCustomer = await buildTestCustomer(client);

                //Attemp to get that TestCustomer from the db
                HttpResponseMessage response = await client.GetAsync($"api/customers/{newTestCustomer.id}");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Customer customer = JsonConvert.DeserializeObject<Customer>(responseBody);
                //validates we get back what we were expecting 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Pete", newTestCustomer.firstName);
                Assert.Equal("Rock", newTestCustomer.lastName);

               await DeleteTestCustomer(newTestCustomer, client);
            }
        }


        //This test attempts to GET a customer based on Id not in the db then confirms NoContent is returned
        [Fact]
        public async Task Test_Get_NonExistant_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
                {
                HttpResponseMessage response = await client.GetAsync("api/customers/999999999");

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            }
        }

        //This Test Creates a new Customer, and validates data matches on firstName and LastName fields then DELETES the newly created customer 
        [Fact]
        public async Task Test_Create_And_Delete_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                Customer newTestCustomer = await buildTestCustomer(client);

                Assert.Equal("Pete", newTestCustomer.firstName);
                Assert.Equal("Rock", newTestCustomer.lastName);

                await DeleteTestCustomer(newTestCustomer, client);
            }
        }

        //This Test attempts to DELETE a Customer based on an Id not in the db then confirms that Customer is NotFound 
        [Fact]
        public async Task Test_Delete_NonExistant_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/customers/900000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }


        //This test creates a new Customer, PUTs new firstName value in database, performs a GET against the newly created Customer by ID and validates the new firstName value changed, then DELETES the newly created Customer
        [Fact]
        public async Task Test_Modify_Customer()
        {
            //assing new firstName to string
            string newFirstName = "Dwayne";
            
            using (HttpClient client = new APIClientProvider().Client)
            {
                //create new Customer
                Customer newTestCustomer = await buildTestCustomer(client);
                //assign new firstName value
                newTestCustomer.firstName = newFirstName;

                string modifiedTestCustomerAsJson = JsonConvert.SerializeObject(newTestCustomer);
                //PUT updated Customer by Id to database
                HttpResponseMessage response = await client.PutAsync(
                    $"api/customers/{newTestCustomer.id}",
                    new StringContent(modifiedTestCustomerAsJson, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                //GET updated Customer info from database by Customer Id
                HttpResponseMessage getTestCustomer = await client.GetAsync($"api/customers/{newTestCustomer.id}");
                getTestCustomer.EnsureSuccessStatusCode();

                string getTestCustomerBody = await getTestCustomer.Content.ReadAsStringAsync();
                Customer modifiedTestCustomer = JsonConvert.DeserializeObject<Customer>(getTestCustomerBody);
                
                Assert.Equal(HttpStatusCode.OK, getTestCustomer.StatusCode);
                //Validate new firstName value was updated
                Assert.Equal(newFirstName, modifiedTestCustomer.firstName);

                await DeleteTestCustomer(modifiedTestCustomer, client);
            }
        }
    }
}