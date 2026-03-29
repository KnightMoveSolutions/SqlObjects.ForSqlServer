using KnightMoves.SqlObjects.ForSqlServer.Configuration;
using KnightMoves.SqlObjects.ForSqlServer.Model;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests;

public class IntegrationTests
{
    // NOTE: Ensure that a local SQL Server instance is running with the Northwind database restored.

    //[Fact]
    [Fact(Skip = "Integration Test. Run manually")]
    public async Task DefaultSchemaLoader_LoadSchemasAsync_Should_Load_Localhost_Northwind_Database_SqlObjects()
    {    
        // ARRANGE
        var options = new SqlObjectsForSqlServerOptions 
        { 
            Databases = new Dictionary<string, DatabaseConfig>
            {
                ["Northwind"] = new DatabaseConfig
                {
                    ConnectionString = "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;",
                    Schemas = ["dbo"]
                }
            }
        };

        Func<string, DbConnection> connectionFactory = (connStr) => new SqlConnection(connStr);
        Func<string, DbConnection, DbCommand> commandFactory = (sql, conn) => 
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        };

        var loader = new DefaultSchemaLoader(options, connectionFactory, commandFactory, new DbCommandExecutor());

        var sqlServerObjects = new SqlServerObjects();

        // ACT
        await loader.LoadSchemasAsync(sqlServerObjects);

        // ASSERT
        Assert.NotNull(sqlServerObjects.Databases);
        Assert.NotEmpty(sqlServerObjects.Databases);
        Assert.Contains(sqlServerObjects.Databases, d => d.Name == "Northwind");

        var northwindDb = sqlServerObjects.Databases.First(d => d.Name == "Northwind");

        Assert.NotNull(northwindDb);
        Assert.NotNull(northwindDb.Schemas);
        Assert.NotEmpty(northwindDb.Schemas);
        Assert.Contains(northwindDb.Schemas, s => s.Name == "dbo");

        var dboSchema = northwindDb.Schemas.First(s => s.Name == "dbo");

        Assert.NotNull(dboSchema);
        Assert.NotNull(dboSchema.Tables);
        Assert.NotEmpty(dboSchema.Tables);
        Assert.Equal(13, dboSchema.Tables.Where(t => t.IsView == false).ToList().Count);
        Assert.Equal(16, dboSchema.Tables.Where(t => t.IsView == true).ToList().Count);

        Assert.NotEmpty(dboSchema.Tables.Where(t => t.Columns.Count > 0));
    }
}