`Documentation Home <https://docs.knightmovesolutions.com>`_

===========
Quick Start
===========

-------
Install
-------

.. code-block:: powershell

   Install-Package KnightMoves.SqlObjects.ForSqlServer

-----
Setup
-----

.. code-block:: csharp

   using KnightMoves.SqlObjects.ForSqlServer;

   ...
   
   var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

   var connStrBuilder = new SqlConnectionStringBuilder(connStr);

   // There are other ways to set this up through 
   // Configuration such as appsettings.json
   builder.Services.AddSqlObjectsForSqlServer(options =>
   {
      options.Databases.Add(
        connStrBuilder.InitialCatalog,
        new DatabaseConfig 
        {
          ConnectionString = connStrBuilder.ConnectionString,
          Schemas = [ "dbo" ]
        }
      );
   });

   ...

   var app = builder.Build();

   ...

   app.UseSqlServerObjectsForSqlServer();

   ...

   app.Run();


-------------------------------
Usage
-------------------------------

To use the metadata in your code, you can inject the ``SqlServerObjects`` class into your services and 
pull the metadata from it. 

.. code-block:: csharp

   using Dapper;
   using KnightMoves.SqlObjects;
   using KnightMoves.SqlObjects.ForSqlServer.Model;

   ...

   public class MyService
   {
      private readonly SqlServerObjects _sqlServerObjects;
      private readonly IDbConnection _connection;

      public MyService(SqlServerObjects sqlServerObjects, IDbConnection connection)
      {
         _sqlServerObjects = sqlServerObjects;
         _connection = connection;
      }

      ...

      public async Task<Customer> GetCustomerAsync(int customerId)
      {

         var customerTableCols = _sqlServerObjects.GetColumns("Customers", "dbo")
                                                  .ForSelect()
                                                  .ToColumnNames();

         var sql = TSQL

            .SELECT()
             .COLUMNS(customerTableCols)
            .FROM("dbo", "Customers")
            .WHERE("CustomerId").IsEqualTo("@CustomerId")
            .Build()
            
        ;

        // Use the sql string with Dapper, Entity Framework, or any other data access technology
        // that accepts raw SQL strings to execute the query and return the results.

        var paramObj = new { CustomerId = customerId };

        var customer = await _connection.QueryAsync<Customer>(sql, paramObj);

        return customer;
      }


   }