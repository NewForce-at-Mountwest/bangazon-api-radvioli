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

    public class TestProduct
    {

        // Create a new Product in the database and makes sure we get a 200 OK status code back
        public async Task<Product> createDrink(HttpClient client)
        {
            Product drink = new Product
            {
                price = 5,
                title = "Drink Thing",
                description = "Description of Drink Thing",
                quantity = 8,
                ProductTypeId = 1,
                CustomerId = 2
            };
            string drinkAsJSON = JsonConvert.SerializeObject(drink);


            HttpResponseMessage response = await client.PostAsync(
                "api/products",
                new StringContent(drinkAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Product newDrink = JsonConvert.DeserializeObject<Product>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newDrink;

        }

        // Delete a Product in the database and make sure we get a no content status code back
        public async Task deleteDrink(Product drink, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/products/{drink.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Products()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all Products; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/products");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Product instances
                List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                // Checks for status 200 OK
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Checks to make sure there are Products in the list
                Assert.True(productList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Product()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product
                Product newProduct = await createDrink(client);

                // Tryies to find the new Product in the database
                HttpResponseMessage response = await client.GetAsync($"api/products/{newProduct.id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Product drink = JsonConvert.DeserializeObject<Product>(responseBody);

                // Checks to make sure we get back what we intended
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(5, newProduct.price);
                Assert.Equal("Drink Thing", newProduct.title);
                Assert.Equal("Description of Drink Thing", newProduct.description);
                Assert.Equal(8, newProduct.quantity);
                Assert.Equal(1, newProduct.ProductTypeId);
                Assert.Equal(2, newProduct.CustomerId);


                // Cleans up the new entry by deleting it
                await deleteDrink(newProduct, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Product_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Product with an improbable Id
                HttpResponseMessage response = await client.GetAsync("api/products/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Product()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Product
                Product newProduct = await createDrink(client);

                // Make sure the info matches what was created
                Assert.Equal(5, newProduct.price);
                Assert.Equal("Drink Thing", newProduct.title);
                Assert.Equal("Description of Drink Thing", newProduct.description);
                Assert.Equal(8, newProduct.quantity);
                Assert.Equal(1, newProduct.ProductTypeId);
                Assert.Equal(2, newProduct.CustomerId);

                // Cleans up the new entry by deleting it
                await deleteDrink(newProduct, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Product_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id with an improbable integer
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/products/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Product()
        {
            //Sets up a string that will replace the existing Title
            string newTitle = "Other Drink Thing";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product
                Product newProduct = await createDrink(client);

                // Change the Title with the previously made string
                newProduct.title = newTitle;

                // Convert them to JSON
                string modifiedDrinkAsJSON = JsonConvert.SerializeObject(newProduct);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/products/{newProduct.id}",
                    new StringContent(modifiedDrinkAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Should send back a 'No Content' response
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the edited Product
                HttpResponseMessage getDrink = await client.GetAsync($"api/products/{newProduct.id}");
                getDrink.EnsureSuccessStatusCode();

                string getDrinkBody = await getDrink.Content.ReadAsStringAsync();
                Product modifiedDrink = JsonConvert.DeserializeObject<Product>(getDrinkBody);

                Assert.Equal(HttpStatusCode.OK, getDrink.StatusCode);

                // Cleans up the new entry by deleting it
                Assert.Equal(newTitle, modifiedDrink.title);

                // Cleans up the new entry by deleting it
                await deleteDrink(modifiedDrink, client);
            }
        }
    }
}