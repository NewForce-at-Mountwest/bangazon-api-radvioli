using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;

namespace TestBangazonAPI
{

    public class TestOrder
    {

        // Create a new Computer in the database and makes sure we get a 200 OK status code back
        public async Task<Order> createNewOrder(HttpClient client)
        {
            Order orderType = new Order
            {
                CustomerId = 2,
                PaymentTypeId = 2
            };
            string computerAsJSON = JsonConvert.SerializeObject(orderType);


            HttpResponseMessage response = await client.PostAsync(
                "api/orders",
                new StringContent(computerAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Order newOrder = JsonConvert.DeserializeObject<Order>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newOrder;

        }

        // Delete a Computer in the database and make sure we get a no content status code back
        public async Task deleteOrder(Order Order, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/orders/{Order.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Orders()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all Computers; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/orders");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Computers instances
                List<Order> orderList = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                // Checks for status 200 OK
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Checks to make sure there are Computers in the list
                Assert.True(orderList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Order()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Order newOrder = await createNewOrder(client);

                // Tryies to find the new Computer in the database
                HttpResponseMessage response = await client.GetAsync($"api/orders/{newOrder.id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order Order = JsonConvert.DeserializeObject<Order>(responseBody);

                // Checks to make sure we get back what we intended
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, Order.CustomerId);
                Assert.Equal(2, Order.PaymentTypeId);



                // Cleans up the new entry by deleting it
                await deleteOrder(Order, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Order_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Computer with an improbable Id
                HttpResponseMessage response = await client.GetAsync("api/orders/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Order()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Order newOrder = await createNewOrder(client);

                // Make sure the info matches what was created

                Assert.Equal(2, newOrder.CustomerId);
                Assert.Equal(2, newOrder.PaymentTypeId);

                // Cleans up the new entry by deleting it
                await deleteOrder(newOrder, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Order_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id with an improbable integer
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/orders/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Order()
        {
            //Sets up a string that will replace the existing Make
            int newCustomerId = 3;

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Order newOrder = await createNewOrder(client);

                // Change the Make with the previously made string
                newOrder.CustomerId = newCustomerId;

                // Convert them to JSON
                string modifiedOrderAsJSON = JsonConvert.SerializeObject(newOrder);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/orders/{newOrder.id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Should send back a 'No Content' response
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the edited Computer
                HttpResponseMessage getOrder = await client.GetAsync($"api/orders/{newOrder.id}");
                getOrder.EnsureSuccessStatusCode();

                string getOrderBody = await getOrder.Content.ReadAsStringAsync();
                Order modifiedOrder = JsonConvert.DeserializeObject<Order>(getOrderBody);

                Assert.Equal(HttpStatusCode.OK, getOrder.StatusCode);

                // Cleans up the new entry by deleting it
                Assert.Equal(newCustomerId, modifiedOrder.CustomerId);

                // Cleans up the new entry by deleting it
                await deleteOrder(modifiedOrder, client);
            }
        }
    }
}
