`Documentation Home <https://docs.knightmovesolutions.com>`_

========
Usage
========

Steps to use the metadata in you code are pretty straightforward. 

- Inject the ``SqlServerObjects`` class into your services 
- Use it to get the metadata for your database tables and columns

---------------------------
Inject ``SqlServerObjects``
---------------------------

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

      public async Task DoSomething()
      {
         // Get the column names for the Customers table in the default schema (dbo)

         var customerColumns = _sqlServerObjects.GetColumns("Customers")
                                                .ForSelect()
                                                .ToColumnNames();
         
         // Use the KnightMoves.SqlObjects Query Builder to build a SQL string with the column names
         var sql = TSQL
         
           .SELECT()
             .COLUMNS(customerColumns)
           .FROM("Customers")
         
         ;

         var customers = await _connection.QueryAsync<Customer[]>(sql);
      }
   }

-----------------------
The GetColumns() Method
-----------------------

The ``GetColumns()`` method is used to get the column metadata for a specific table. It accepts the name of the table and optionally the schema name
and database name (in case you're using multiple databases). If the schema name is not provided, it defaults to ``dbo``. If the database name is not 
provided, it defaults to the first database in the configuration (which may be the only database if you have only one configured).

.. code-block:: csharp 

   // Get the column metadata for the Customers table in the default schema (dbo) and default database
   var customerColumns = _sqlServerObjects.GetColumns("Customers");

   // Get the column metadata for the Orders table in the reporting schema and default database
   var orderColumns = _sqlServerObjects.GetColumns("Orders", "reporting");
   
   // Get the column metadata for the Products table in the default schema (dbo) and a specific database
   var productColumns = _sqlServerObjects.GetColumns("Products", "dbo", "InventoryDB");

The ``GetColumns()`` method returns a collection of ``SqlServerColumn`` objects, which contain the metadata for each column in the specified table.

- Columns are always returned in the order they are defined in the database, which is important for SQL statements where the order of columns matters

----------------------
The ForSelect() Method
----------------------

The ``ForSelect()`` method is an extension method that returns the collection of ``SqlServerColumn`` objects that is suitable for use in a SELECT statement.

.. code-block:: csharp 

   var customerColumns = _sqlServerObjects.GetColumns("Customers")
                                          .ForSelect();

- Columns are always returned in the order they are defined in the database, which is important for SQL statements where the order of columns matters

----------------------
The ForInsert() Method
----------------------

The ``ForInsert()`` method is an extension method that returns the collection of ``SqlServerColumn`` objects that is suitable for use in an INSERT statement.
Most importantly, it excludes columns that are identity columns, which cannot be included in an INSERT statement.

.. code-block:: csharp 

   var customerColumns = _sqlServerObjects.GetColumns("Customers")
                                          .ForInsert();

- Columns are always returned in the order they are defined in the database, which is important for SQL statements where the order of columns matters

----------------------
The ForUpdate() Method
----------------------

The ``ForUpdate()`` method is an extension method that returns the collection of ``SqlServerColumn`` objects that is suitable for use in an UPDATE statement.
Most importantly, it excludes columns that are identity columns, which cannot be included in the main body of an UPDATE statement.

.. code-block:: csharp 

   var customerColumns = _sqlServerObjects.GetColumns("Customers")
                                          .ForInsert();

- Columns are always returned in the order they are defined in the database, which is important for SQL statements where the order of columns matters

--------------------------
The ToColumnNames() Method
--------------------------

The ``ToColumnNames()`` method is an extension method that returns a collection of column names from a collection of ``SqlServerColumn`` objects as a 
collection of strings. This is useful for building SQL statements where you need the column names as strings mainly for including in the SQL string itself.

.. code-block:: csharp 

   var customerColumnNames = _sqlServerObjects.GetColumns("Customers")
                                              .ForSelect()
                                              .ToColumnNames()
   
   // customerColumnNames is of type List<string> so now you can use it to build a SQL string
   var sql = TSQL
   
     .SELECT()
       .COLUMNS(customerColumns)
     .FROM("Customers")
   
   ;