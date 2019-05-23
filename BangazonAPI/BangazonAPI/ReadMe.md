Bangazon API

You can find the ERD here:
https://dbdiagram.io/d/5cdbeaf51f6a891a6a654e23

You'll also need to set up a database for this project. To do this, open SSMS and click the option to create a new database. Name your database BangdaRadvioli and go to the following link to get the information you'll need
https://github.com/NewForce-at-Mountwest/bangazon-inc/blob/master/book-2-platform-api/chapters/sql/bangazon.sql

Once you have the information copied, go to your newly created database and click New Query and paste in the information you copied.
Run the query to set up the database.

How To Use The Bangazon API

Clone the files from Github and launch Visual Studio. Open a new project, select .Net Core from the menu, then ASP.NET Core Web Application from the list of options. Name the project and choose API from the list of options. Click ok to create the project.

Go to your appsettings.json file and replace the code inside with this:
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=BangaRadvioli;Trusted_Connection=True;"
  }
}

To enable the ability to test these files, you'll need to create a test project. To do this:

Right-click on solution and select add > New Project
Make sure you have .Net Core selected and choose xUnit Test Project from list of options
Name the test project TestBangazonAPI and click ok.

Dependencies

You'll want to install the SQL package to allow you to access the SQL server:

cd BangazonAPI
dotnet add package System.Data.SqlClient
dotnet restore

Now you'll install the packages for the test project:

cd ../TestBangazonAPI
dotnet add package Microsoft.AspNetCore
dotnet add package Microsoft.AspNetCore.HttpsPolicy
dotnet add package Microsoft.AspNetCore.Mvc
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet restore

To test the files, scroll down in the solution explorer to the TestBangazonAPI folder, right click it and click run tests.
The results should show on the left side of the screen as successful.

Features

Employee
Includes an employee model with the following information: FirstName, LastName, isSuperVisor, DepartmentId, ComputerId, EmployeeDepartment, and EmployeeComputer
The Employee Controller has functionality to view a single employee, view all employees, add an employee, or edit an employee in the database.
Summary of employee functionality:
This gives the user the ability to view a single employee or all employees as well as add, and edit employees in the database. 

Payment Type
Includes a model with name, account number, CustomerId
The payment type controller has functionality to view a single payment type, view all payment types, add a payment type, edit a payment type, or delete a payment type.
Summary of payment type functionality:
This gives the user the ability to view one or all payment types as well as add, edit, and delete payment types.

Contributors
Matt Rowe
Nikki Ash
David Bird
Dale Saul