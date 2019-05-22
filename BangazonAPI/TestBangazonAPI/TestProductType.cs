using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestBangazonAPI
{

    public class TestProductType
    {

        // Create a new Product Type in the database and makes sure we get a 200 OK status code back
        public async Task<ProductType> createDrinkType(HttpClient client)
        {
            ProductType drinkType = new ProductType
            {
                name = "Drinks",
            };
            string drinkAsJSON = JsonConvert.SerializeObject(drinkType);


            HttpResponseMessage response = await client.PostAsync(
                "api/producttypes",
                new StringContent(drinkAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            ProductType newDrinkType = JsonConvert.DeserializeObject<ProductType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newDrinkType;

        }

        // Delete a Product Type in the database and make sure we get a no content status code back
        public async Task deleteDrinkType(ProductType drinkType, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/producttypes/{drinkType.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all Products Types; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/producttypes");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Product Type instances
                List<ProductType> productTypeList = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                // Checks for status 200 OK
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Checks to make sure there are Product Types in the list
                Assert.True(productTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product Type
                ProductType newProductType = await createDrinkType(client);

                // Tryies to find the new Product Type in the database
                HttpResponseMessage response = await client.GetAsync($"api/producttypes/{newProductType.id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType drinkType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Checks to make sure we get back what we intended
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Drinks", newProductType.name);



                // Cleans up the new entry by deleting it
                await deleteDrinkType(newProductType, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Product_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Product Type with an improbable Id
                HttpResponseMessage response = await client.GetAsync("api/producttypes/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Product Type
                ProductType newProductType = await createDrinkType(client);

                // Make sure the info matches what was created
                Assert.Equal("Drinks", newProductType.name);

                // Cleans up the new entry by deleting it
                await deleteDrinkType(newProductType, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id with an improbable integer
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/producttypes/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_ProductType()
        {
            //Sets up a string that will replace the existing Name
            string newName = "New Drinks";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product Type
                ProductType newProductType = await createDrinkType(client);

                // Change the Name with the previously made string
                newProductType.name = newName;

                // Convert them to JSON
                string modifiedDrinkTypeAsJSON = JsonConvert.SerializeObject(newProductType);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/producttypes/{newProductType.id}",
                    new StringContent(modifiedDrinkTypeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Should send back a 'No Content' response
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the edited Product Type
                HttpResponseMessage getDrinkType = await client.GetAsync($"api/producttypes/{newProductType.id}");
                getDrinkType.EnsureSuccessStatusCode();

                string getDrinkTypeBody = await getDrinkType.Content.ReadAsStringAsync();
                ProductType modifiedDrinkType = JsonConvert.DeserializeObject<ProductType>(getDrinkTypeBody);

                Assert.Equal(HttpStatusCode.OK, getDrinkType.StatusCode);

                // Cleans up the new entry by deleting it
                Assert.Equal(newName, modifiedDrinkType.name);

                // Cleans up the new entry by deleting it
                await deleteDrinkType(modifiedDrinkType, client);
            }
        }
    }
}