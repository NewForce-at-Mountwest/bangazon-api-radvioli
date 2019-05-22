using Newtonsoft.Json;
using TestBangazonAPI;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestBangazonAPI
{
    public class PaymentTypeTest
    {


        // Creating new payment type and checking for ok code

        public async Task<PaymentType> createPaymentType(HttpClient client)
        {
            PaymentType paymentType = new PaymentType
            {
                accountNumber = 987123,
                name = "Test Payment Type",
                CustomerId = 1


            };
            string paymentTypeAsJSON = JsonConvert.SerializeObject(paymentType);


            HttpResponseMessage response = await client.PostAsync(
                "api/paymentType",
                new StringContent(paymentTypeAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentType newPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newPaymentType;

        }

        // Deleting a payment type and checking for correct result
        public async Task deletePaymentType(PaymentType paymentType, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/paymentType/{paymentType.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task GetAllPaymentTypes()
        {
            
            using (HttpClient client = new APIClientProvider().Client)
            {

                // attempt to get payment types from database
                HttpResponseMessage response = await client.GetAsync("api/paymentType");

                // check for response
                response.EnsureSuccessStatusCode();

                // read response as json
                string responseBody = await response.Content.ReadAsStringAsync();

                // convert json to c#
                List<PaymentType> paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                // check to make sure expected results came back
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                
                Assert.True(paymentTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task GetOnePaymentType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // create a new paymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // get payment type from database
                HttpResponseMessage response = await client.GetAsync($"api/paymentType/{newPaymentType.id}");

                response.EnsureSuccessStatusCode();

                // get response as json
                string responseBody = await response.Content.ReadAsStringAsync();

                // convert the JSON into C#
                PaymentType paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                // check for expected results
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Payment Type", newPaymentType.name);


                // Clean up after ourselves- delete paymentType!
                deletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task GetNonExistantPaymentTypeFails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a paymentType with huge id
                HttpResponseMessage response = await client.GetAsync("api/paymentType/999999999");

                // check for correct error code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task CreateAndDeletePaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new PaymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // Make sure it worked
                Assert.Equal("Test Payment Type", newPaymentType.name);



                // Clean up after ourselves, delete PaymentType!
                deletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task DeleteNonExistentPaymentTypeFails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete a nonexistant Id
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/paymentType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task ModifyPaymentType()
        {

            // changing payment type name
            string newName = "moms credit card";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new paymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // Change the name
                newPaymentType.name = newName;

                // Convert response to json
                string modifiedPaymentTypeAsJSON = JsonConvert.SerializeObject(newPaymentType);

                // PUT request with updated information
                HttpResponseMessage response = await client.PutAsync(
                    $"api/paymentType/{newPaymentType.id}",
                    new StringContent(modifiedPaymentTypeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // check for no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                
                // GET edited information
                HttpResponseMessage getPaymentType = await client.GetAsync($"api/paymentType/{newPaymentType.id}");
                getPaymentType.EnsureSuccessStatusCode();

                string getPaymentTypeBody = await getPaymentType.Content.ReadAsStringAsync();
                PaymentType modifiedPaymentType = JsonConvert.DeserializeObject<PaymentType>(getPaymentTypeBody);

                Assert.Equal(HttpStatusCode.OK, getPaymentType.StatusCode);

                // Checking that name was updated
                Assert.Equal(newName, modifiedPaymentType.name);

                // Clean up after ourselves- delete it
                deletePaymentType(modifiedPaymentType, client);
            }
        }




    }
}