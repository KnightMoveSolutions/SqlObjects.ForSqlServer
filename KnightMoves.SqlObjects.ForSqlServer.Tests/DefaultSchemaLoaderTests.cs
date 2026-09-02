using KnightMoves.SqlObjects.ForSqlServer.Configuration;
using KnightMoves.SqlObjects.ForSqlServer.Model;
using Moq;
using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests;

public class DefaultSchemaLoaderTests
{
    [Fact]
    public async Task LoadSchemasAsync_LoadsSchemasSuccessfully()
    {
        // ARRANGE
        var testConnString = "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";
        var options = new SqlObjectsForSqlServerOptions 
        { 
            Databases = new()
            {
                ["Northwind"] = new DatabaseConfig { ConnectionString = testConnString, Schemas = ["dbo"] }
            }
        };

        var mockConnection = new Mock<DbConnection>();

        var mockSchemaReader = new Mock<DbDataReader>();
        mockSchemaReader
            .SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        mockSchemaReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(0);
        mockSchemaReader.Setup(r => r.GetString(It.IsAny<int>())).Returns("dbo");

        var mockTableReader = new Mock<DbDataReader>();
        mockTableReader
            .SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockTableReader
            .Setup(r => r.GetString(It.IsAny<int>()))
            .Returns("TestTable");

        var mockViewReader = new Mock<DbDataReader>();
        mockViewReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        mockViewReader
            .Setup(r => r.GetString(It.IsAny<int>()))
            .Returns("TestView");

        var mockColumnReader = new Mock<DbDataReader>();

        mockColumnReader
            .SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        mockColumnReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(0);
        mockColumnReader.Setup(r => r.GetString(It.IsAny<int>())).Returns(string.Empty);
        mockColumnReader.Setup(r => r.GetInt32(It.IsAny<int>())).Returns(1);
        mockColumnReader.Setup(r => r.GetInt16(It.IsAny<int>())).Returns(1);
        mockColumnReader.Setup(r => r.GetByte(It.IsAny<int>())).Returns(1);
        mockColumnReader.SetupSequence(r => r.IsDBNull(It.IsAny<int>()))
            // Use Table Columns for true case
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)
            .Returns(true)

            // Use View Columns for false case
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
            .Returns(false)
        ;

        var mockCommand = new Mock<DbCommand>();

        var mockCommandExecutor = new Mock<IDbCommandExecutor>();
        mockCommandExecutor
            .SetupSequence(cmd => cmd.ExecuteReaderAsync(It.IsAny<DbCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockSchemaReader.Object) // For schemas
            .ReturnsAsync(mockTableReader.Object)  // For tables
            .ReturnsAsync(mockColumnReader.Object) // For table column
            .ReturnsAsync(mockViewReader.Object)  // For views
            .ReturnsAsync(mockColumnReader.Object) // For view columns
        ;

        DbConnection connectionFactory(string connStr) => mockConnection.Object;
        DbCommand commandFactory(string sql, DbConnection conn) => mockCommand.Object;

        var loader = new DefaultSchemaLoader(options, connectionFactory, (Func<string, DbConnection, DbCommand>)commandFactory, mockCommandExecutor.Object);
        var sqlServerObjects = new SqlServerObjects();

        // ACT
        await loader.LoadSchemasAsync(sqlServerObjects, new CancellationToken());

        // ASSERT
        Assert.NotEmpty(sqlServerObjects.Databases);
        Assert.Contains(sqlServerObjects.Databases, x => x.Name == "Northwind");

        var northwindDb = sqlServerObjects.Databases.FirstOrDefault();

        Assert.NotNull(northwindDb);
        Assert.NotEmpty(northwindDb.Schemas);
        Assert.Contains(northwindDb.Schemas, x => x.Name == "dbo");

        var dboSchema = northwindDb.Schemas.FirstOrDefault();

        Assert.NotNull(dboSchema);
        Assert.NotEmpty(dboSchema.Tables);
        Assert.Contains(dboSchema.Tables, x => x.Name == "TestTable");
        Assert.Contains(dboSchema.Tables, x => x.Name == "TestView");

        var dbTable = dboSchema.Tables.FirstOrDefault(t => t.Name == "TestTable");

        Assert.NotNull(dbTable);
        Assert.NotEmpty(dbTable.Columns);
        Assert.Single(dbTable.Columns);

        var dbView = dboSchema.Tables.FirstOrDefault(t => t.Name == "TestView");

        Assert.NotNull(dbView);
        Assert.NotEmpty(dbView.Columns);
        Assert.Single(dbView.Columns);
    }
}
