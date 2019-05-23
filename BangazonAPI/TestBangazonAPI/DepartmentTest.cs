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

    public class DepartmentTest
    {
        //Create a new Department in the db and check for 200 OK status code
        public async Task<Department> buildTestDepartment(HttpClient client)
        {
            Department TestDepartment = new Department
            {
                name = "Advertising",
                budget = 500000
            };

            string TestDepartmentAsJSON = JsonConvert.SerializeObject(TestDepartment);

            HttpResponseMessage response = await client.PostAsync("api/departments", new StringContent(TestDepartmentAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            Department newTestDepartment = JsonConvert.DeserializeObject<Department>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newTestDepartment;

        }

        //Delete the Department you just created and make sure you get no content status code back

        public async Task DeleteTestDepartment(Department TestDepartment, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/departments/{TestDepartment.id}");

            deleteResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        //This TEST gets all Departments from the database
        [Fact]
        public async Task Test_Get_ALL_Departments()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage response = await client.GetAsync("api/departments");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.True(departmentList.Count > 0);
            }
        }

        //This test Creates a new Department, performs a GET for single Department just created based on department Id and validates data matches on Name and budget fields, then deletes the newly created Department 
        [Fact]
        public async Task Test_Get_Single_Department()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                //Create a new TestDepartment
                Department newTestDepartment = await buildTestDepartment(client);

                 //Attempt to get the TestDepartment from the database   
                 HttpResponseMessage response = await client.GetAsync($"api/departments/{newTestDepartment.id}");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Department department = JsonConvert.DeserializeObject<Department>(responseBody);

                //Validates that we get back what we were expecting
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.Equal("Advertising", newTestDepartment.name);

                Assert.Equal(500000, newTestDepartment.budget);

                await DeleteTestDepartment(newTestDepartment, client);

            }
        }

        //This test attempts to GET a Department based on ID not in the database then confirms NoContent is returned
        [Fact]
        public async Task Test_Get_NonExistant_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                HttpResponseMessage response = await client.GetAsync("api/departments/99999999");

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //This test Creates a new Department, and validates data matches on name and budget fields then DELETES the newly created Department
        [Fact]
        public async Task Test_Create_And_Delete_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                Department newTestDepartment = await buildTestDepartment(client);

                Assert.Equal("Advertising", newTestDepartment.name);

                Assert.Equal(500000, newTestDepartment.budget);

                await DeleteTestDepartment(newTestDepartment, client);
            }
        }

        //This test attempts to DELETE a Department based on an Id that is not in the database then confirms that Department is NotFound
        [Fact]
        public async Task Test_Delete_NonExistant_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync("api/departments/9999999");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        //This test creates a new Department, PUTs new Name value in database, performs a GET against the newly created Department by ID and validates the new Name value was changed, before deleting the newly created Department
        [Fact]

        public async Task Test_Modify_Customer()
        {
            //Assign new department name 
            string newName ="Public Relations";

            using (HttpClient client = new APIClientProvider().Client)
            {
                //create new Department
                Department newTestDepartment = await buildTestDepartment(client);
                //assigne new Name value
                newTestDepartment.name = newName;

                string modifiedTestDepartmentAsJson = JsonConvert.SerializeObject(newTestDepartment);
                //PUT updated Department by Id into database
                HttpResponseMessage response = await client.PutAsync($"api/departments/{newTestDepartment.id}", new StringContent(modifiedTestDepartmentAsJson, Encoding.UTF8, "application/json")
                    );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                //Get updated Department info from database by department Id
                HttpResponseMessage getTestDepartment = await client.GetAsync($"api/departments/{newTestDepartment.id}");
                getTestDepartment.EnsureSuccessStatusCode();

                string getTestDepartmentBody = await getTestDepartment.Content.ReadAsStringAsync();

                Department modifiedTestDepartment = JsonConvert.DeserializeObject<Department>(getTestDepartmentBody);

                Assert.Equal(HttpStatusCode.OK, getTestDepartment.StatusCode);
                //Validate new name value was updated
                Assert.Equal(newName, modifiedTestDepartment.name);

                await DeleteTestDepartment(modifiedTestDepartment, client);

            }
        }
    }

}
