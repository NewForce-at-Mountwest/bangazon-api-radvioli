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

    public class TestComputer
    {

        // Create a new Computer in the database and makes sure we get a 200 OK status code back
        public async Task<Computer> createNewComputer(HttpClient client)
        {
            Computer computerType = new Computer
            {
                PurchaseDate = DateTime.Now,
                DecomissionDate = DateTime.Now,
                make = "Laptop Epsilon",
                manufacturer = "Dell"
            };
            string computerAsJSON = JsonConvert.SerializeObject(computerType);


            HttpResponseMessage response = await client.PostAsync(
                "api/computers",
                new StringContent(computerAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Computer newComputer = JsonConvert.DeserializeObject<Computer>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newComputer;

        }

        // Delete a Computer in the database and make sure we get a no content status code back
        public async Task deleteComputer(Computer Computer, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/computers/{Computer.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Computers()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all Computers; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/computers");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Computers instances
                List<Computer> computerList = JsonConvert.DeserializeObject<List<Computer>>(responseBody);

                // Checks for status 200 OK
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Checks to make sure there are Computers in the list
                Assert.True(computerList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Computer()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Computer newComputer = await createNewComputer(client);

                // Tryies to find the new Computer in the database
                HttpResponseMessage response = await client.GetAsync($"api/computers/{newComputer.id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Computer Computer = JsonConvert.DeserializeObject<Computer>(responseBody);

                // Checks to make sure we get back what we intended
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Laptop Epsilon", Computer.make);
                Assert.Equal("Dell", Computer.manufacturer);



                // Cleans up the new entry by deleting it
                await deleteComputer(Computer, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Computer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Computer with an improbable Id
                HttpResponseMessage response = await client.GetAsync("api/computers/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Computer()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Computer newComputer = await createNewComputer(client);

                // Make sure the info matches what was created

                Assert.Equal("Laptop Epsilon", newComputer.make);
                Assert.Equal("Dell", newComputer.manufacturer);

                // Cleans up the new entry by deleting it
                await deleteComputer(newComputer, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Computer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id with an improbable integer
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/computers/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Computer()
        {
            //Sets up a string that will replace the existing Make
            string newMake = "New Laptop Epsilon";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Computer newComputer = await createNewComputer(client);

                // Change the Make with the previously made string
                newComputer.make = newMake;

                // Convert them to JSON
                string modifiedComputerAsJSON = JsonConvert.SerializeObject(newComputer);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/computers/{newComputer.id}",
                    new StringContent(modifiedComputerAsJSON, Encoding.UTF8, "application/json")
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
                HttpResponseMessage getComputer = await client.GetAsync($"api/computers/{newComputer.id}");
                getComputer.EnsureSuccessStatusCode();

                string getComputerBody = await getComputer.Content.ReadAsStringAsync();
                Computer modifiedComputer = JsonConvert.DeserializeObject<Computer>(getComputerBody);

                Assert.Equal(HttpStatusCode.OK, getComputer.StatusCode);

                // Cleans up the new entry by deleting it
                Assert.Equal(newMake, modifiedComputer.make);

                // Cleans up the new entry by deleting it
                await deleteComputer(modifiedComputer, client);
            }
        }
    }
}