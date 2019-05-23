using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TestBangazonAPI;
using System.Linq;
using BangazonAPI.Models;

namespace TestBangazonAPI
{

    public class EmployeeTest
    {


        public async Task<Employee> createEmployee(HttpClient client)
        {
            //making my cat an employee for testing purposes
            Employee employee = new Employee
            {
                firstName = "Andy",
                lastName = "Ash",
                isSupervisor = true,
                DepartmentId = 1,

            };
            //convert the cat to json
            string employeeAsJSON = JsonConvert.SerializeObject(employee);

            
            HttpResponseMessage response = await client.PostAsync(
                "api/Employees",
                new StringContent(employeeAsJSON, Encoding.UTF8, "application/json")
            );
            //check to see if cat was hired
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            //convert back into c#
            Employee newEmployee = JsonConvert.DeserializeObject<Employee>(responseBody);

            //checking for status code
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newEmployee;

        }

        // Fire my cat and make sure he's deleted from DB
        public async Task deleteEmployee(Employee employee, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Employees/{employee.id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task GetAllEmployees()
        {
            // Using http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // get all employees
                HttpResponseMessage response = await client.GetAsync("api/Employees");

                // check for response
                response.EnsureSuccessStatusCode();

                // read response as json
                string responseBody = await response.Content.ReadAsStringAsync();

                // convert json to c#
                List<Employee> EmployeeList = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                // get that status code
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // check for employees
                Assert.True(EmployeeList.Count > 0);
            }
        }

        [Fact]
        public async Task GetSingleEmployee()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // create employee
                Employee newEmployee = await createEmployee(client);

                // fetch that employee
                HttpResponseMessage response = await client.GetAsync($"api/Employees/{newEmployee.id}");

                response.EnsureSuccessStatusCode();

                // convert to json
                string responseBody = await response.Content.ReadAsStringAsync();

                // convert to c#
                Employee Employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                // check to see if right response came back
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Andy", newEmployee.firstName);

                // fire the cat
                await deleteEmployee(newEmployee, client);
            }
        }

        [Fact]
        public async Task GetNonExistentEmployeeFails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // attempt to grab nonexistant employee
                HttpResponseMessage response = await client.GetAsync("api/Employees/123456");

                // check for 204 code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task CreateAndDeleteEmployee()
        {
            using (var client = new APIClientProvider().Client)
            {

                // hire cat
                Employee newEmployee = await createEmployee(client);

                // check response
                Assert.Equal("Andy", newEmployee.firstName);


                // fire cat, again
                await deleteEmployee(newEmployee, client);
            }
        }

        [Fact]
        public async Task DeleteNonExistentEmployeeFails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // try to delete an employee that doesn't exist
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Employees/987654");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task EditEmployee()
        {

            // attempt to edit an employees name
            string editedName = "Andrew";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // create employee
                Employee newEmployee = await createEmployee(client);

                // change name of employee
                newEmployee.firstName = editedName;

                // convert response to json
                string editedEmployeeAsJSON = JsonConvert.SerializeObject(newEmployee);

                // PUT request
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Employees/{newEmployee.id}",
                    new StringContent(editedEmployeeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // convert that to json
                string responseBody = await response.Content.ReadAsStringAsync();

                // check for no content code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                HttpResponseMessage getEmployee = await client.GetAsync($"api/Employees/{newEmployee.id}");
                getEmployee.EnsureSuccessStatusCode();

                string getEmployeeBody = await getEmployee.Content.ReadAsStringAsync();
                Employee editedEmployee = JsonConvert.DeserializeObject<Employee>(getEmployeeBody);

                Assert.Equal(HttpStatusCode.OK, getEmployee.StatusCode);


                Assert.Equal(editedName, editedEmployee.firstName);

                // fire the cat for the last time
                await deleteEmployee(editedEmployee, client);
            }
        }
    }
}
