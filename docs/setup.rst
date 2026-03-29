`Documentation Home <https://docs.knightmovesolutions.com>`_

========
Setup
========

There are multiple ways to set up the use of this library. Below are the various ways to set it up. 

---------------------------------
Using Configuration (Recommended)
---------------------------------

This is the recommended way to set up the library. It allows you to set up the library through configuration such 
as ``appsettings.json``, which can be more maintainable and easier to manage in different environments.

.. code-block:: json 
 
   {
     "ConnectionStrings": {
       "Default": "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True;"
     },
     "SqlObjectsForSqlServerOptions": {
       "Databases": {
         "Default": null
       }
     }
   }

This is the easiest way to setup the library, which exhibits the following specs:

- The ``ConnectionStrings`` section is the standard place to configure connection strings. 
  Typically, the connection string is a secret that is stored in a secure location such as Azure Key Valut etc. 
  but for the sake of this example, we are including it in the ``appsettings.json`` file.

- The ``SqlObjectsForSqlServerOptions`` section is where you configure the options for the library.
  Under the ``Databases`` section, you can specify the name of the database and the configuration for that database.
  The name of the database should match the name of the connection string in the ``ConnectionStrings`` section. In 
  this example, we are using ``Default`` as the name of the database and the connection string. 

- A ``null`` value for the ``Default`` database section means that the library will use the connection string from the 
  ``ConnectionStrings`` section with the same name. In this example, the library will use the connection string from the 
  ``ConnectionStrings:Default`` section. It also means that the library will use the default schema, which is ``dbo``. 
  If you want to specify different schemas, you can do so by providing a configuration object instead of ``null``.


If you don't want to use the defaults, you can provide a configuration object for the database instead of ``null``. 
This allows you to specify different connection strings and schemas for the database.

.. code-block:: json 
 
   {
     "SqlObjectsForSqlServerOptions": {
       "Databases": {
         "Default": {
           "ConnectionString": "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True;",
           "Schemas": [ "dbo", "reporting" ]
         }
       }
     }
   }

- If you omit the ``ConnectionString`` it will take from the ``ConnectionStrings`` section with the same name. 
- If you omit the ``Schemas`` it will default to ``dbo``
- If you use the ``Schemas`` property to set a different schema, such as ``reporting``, then you must also explicitly add 
  ``dbo`` to the list of schemas if you want to include it as well. The library does not include the default schema if 
  you specify a different schema.

Register and Use the Library
----------------------------

.. code-block:: csharp 

   using KnightMoves.SqlObjects.ForSqlServer;
   
   ...

   // This works when using the configuration approach above
   builder.Services.AddSqlObjectsForSqlServer(builder.Configuration);

   ...
   
   var app = builder.Build();
   
   ...
   
   // This loads the metadata for the databases in configuration
   app.UseSqlServerObjectsForSqlServer();
   
   ...
   
   app.Run();


---------------
Explicit Setup 
---------------

If you don't want to use the configuration approach, you can set up the library explicitly in code. This allows you to have 
more control over the setup process and can be useful in certain scenarios.

.. code-block:: csharp 

  using KnightMoves.SqlObjects.ForSqlServer;
   
   ...
   
   var connStr = builder.Configuration.GetConnectionString("Default");
   
   var connStrBuilder = new SqlConnectionStringBuilder(connStr);
   
   // Instead of using the configuration approach you do the following
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
   
   // This loads the metadata for the databases in configuration
   app.UseSqlServerObjectsForSqlServer();
   
   ...
   
   app.Run();

- If the connection string key is the same as the name of the connection string in the ``ConnectionStrings`` section, 
  then you can omit the ``ConnectionString`` property in the configuration object and it will take it from the ``ConnectionStrings`` 
  section. The connection string key in the example above is set to ``connStrBuilder.InitialCatalog``. You can explicitly set it to 
  something like ``Default`` to match the name of the connection string in the ``ConnectionStrings`` section if you want to use that feature.

- If you omit the ``Schemas`` property, it will default to ``dbo``. If you want to specify different schemas, you can do so by providing a 
  list of schemas in the configuration object.


-----
Other
-----

There are other ways to create the ``options`` object, which is of type ``SqlObjectsForSqlServerOptions`` and pass the options object 
to the ``AddSqlObjectsForSqlServer`` method. 

Use the Factory Method to Pull from Configuration
-------------------------------------------------

.. code-block:: csharp 

   ...

   var options = SqlObjectsForSqlServerOptions.Create(builder.Configuration);

   builder.Services.AddSqlObjectsForSqlServerOptions(options);

   ...

Use the ``IConfiguration`` Extension Method to Pull from Configuration 
----------------------------------------------------------------------

.. code-block:: csharp 

   ...

   // It will look for the "SqlObjectsForSqlServerOptions" section in configuration and bind it to the options object
   var options = builder.Configuration.GetSqlObjectsForSqlServerOptions();

   builder.Services.AddSqlObjectsForSqlServerOptions(options);

   ...

Use a Sub-Section of Configuration
----------------------------------

You can put the ``SqlObjectsForSqlServerOptions`` section under a different section in configuration and use the factory method or extension method to pull it out.

For example, suppose you have separate sections for multi-tenancy. 

.. code-block:: json 

   {
     "TenantA": {
       "ConnectionStrings": {
         "Default": "Data Source=localhost;Initial Catalog=Northwind_A;Integrated Security=True;"
       },
       "SqlObjectsForSqlServerOptions": {
         "Databases": {
           "Default": null
         }
       }
     },
     "TenantB": {
       "ConnectionStrings": {
         "Default": "Data Source=localhost;Initial Catalog=Northwind_B;Integrated Security=True;"
       },
       "SqlObjectsForSqlServerOptions": {
         "Databases": {
           "Default": null
         }
       }
     }
   }

You can pull the options for a particular tenant like this using the Factory Method: 

.. code-block:: csharp 

   ...
   
   var config = builder.Configuration.GetSection("TenantA").Get<SqlObjectsForSqlServerOptions>();

   var options = SqlObjectsForSqlServerOptions.Create(config);

   builder.Services.AddSqlObjectsForSqlServerOptions(options);

   ...


Or you can pull the options for a particular tenant like this using the ``IConfiguration`` Extension Method:

.. code-block:: csharp 

   ...
   
   var options = builder.Configuration.GetSqlObjectsForSqlServerOptions("TenantA");

   builder.Services.AddSqlObjectsForSqlServerOptions(options);

   ...

