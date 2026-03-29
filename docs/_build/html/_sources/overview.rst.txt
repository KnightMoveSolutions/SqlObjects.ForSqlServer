`Documentation Home <https://docs.knightmovesolutions.com>`_

========
Overview
========

What is it?
-----------

``KnightMoves.SqlObjects.ForSqlServer`` is a .NET library that extracts SQL Server database table and column metadata and exposes it as C# objects. 
With the metadata available as objects, you can write code that generates SQL queries, documentation, or perform other tasks that require knowledge 
of the database structure.

What problem does it solve?
---------------------------

This library was created to solve the problem of including database table columns in SQL strings meant for submitting to the database for INSERTS, 
UPDATES, and SELECTS. By having the metadata available as objects, you can write code that generates SQL queries without hardcoding column names, 
which can lead to more maintainable and less error-prone code.

When used in combination with the ``KnightMoves.SqlObjects`` library, you can generate SQL queries that are easily used with Dapper, Entity Framework, 
or any other data access technology that accepts raw SQL strings.

Benefits 
--------

- Automate the generation of SQL queries based on the database structure, which can save time and reduce errors. 
  When the database structure changes, the metadata will be updated automatically, so you don't have to worry about updating your SQL strings manually. 

- Columns are returned in the order they are defined in the database, which can be important for certain queries and operations

- The library is designed to be used in combination with the ``KnightMoves.SqlObjects`` library, which provides a fluent API for building SQL queries. 
  This allows you to write code that generates SQL queries in a more readable and maintainable way.

- The library is built with performance in mind, so it caches the metadata to avoid unnecessary database calls and improve performance. 

- The library includes methods to filter columns suitable for SELECT, INSERT, and UPDATE operations, which can help you generate more accurate SQL queries 
  based on the context of your operations. No more worrying about including identity columns in your INSERT statements or trying to remember which columns 
  are nullable when building your UPDATE statements.

- Filter columns based on arbitrary criteria using LINQ, which gives you the flexibility to generate SQL queries that are tailored to your specific needs and requirements. 
  For example, you can filter columns based on their data types, nullability, or any other metadata property that is available in the ``SqlServerColumn`` objects.
