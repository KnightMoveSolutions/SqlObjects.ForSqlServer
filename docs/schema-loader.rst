`Documentation Home <https://docs.knightmovesolutions.com>`_

=============
Schema Loader
=============

The heart of this library is in the Schema Loader, which is the class that contains the code that fetches the metadata from the database 
and loads it into the ``SqlServerObjects`` class. 

If, for whatever reason, you want to extend it or use a custom schema loader, you can implement the ``ISchemaLoader`` interface and register 
it after calling ``builder.Services.AddSqlObjectsForSqlServer()`` to ensure it is used instead of the ``DefaultSchemaLoader`` that 
comes with the library. 

Here's the interface that must be implemented

ISchemaLoader
-----------------

.. code-block:: csharp 

   using KnightMoves.SqlObjects.ForSqlServer.Model;
   
   namespace KnightMoves.SqlObjects.ForSqlServer;
   
   public interface ISchemaLoader
   {
       Task LoadSchemasAsync(SqlServerObjects sqlServerObjects, CancellationToken cancellationToken = default);
   }
   
Once you have implemented the interface, you can register it in the DI container like this:

.. code-block:: csharp 

   builder.Services.AddSingleton<ISchemaLoader, MyCustomSchemaLoader>();
