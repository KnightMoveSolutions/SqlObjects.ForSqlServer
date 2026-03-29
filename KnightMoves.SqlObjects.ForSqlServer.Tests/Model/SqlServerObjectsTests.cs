using KnightMoves.SqlObjects.ForSqlServer.Model;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests.Model;

public class SqlServerObjectsTests
{
    [Fact]
    public void GetColumns_NullOrEmptyTable_ThrowsArgumentNullException()
    {
        // ARRANGE
        var sqlServerObjects = new SqlServerObjects();

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentNullException>(() => sqlServerObjects.GetColumns(null!));
        Assert.Equal("Value cannot be null. (Parameter 'table')", exception.Message);
        exception = Assert.Throws<ArgumentNullException>(() => sqlServerObjects.GetColumns(string.Empty));
        Assert.Equal("Value cannot be null. (Parameter 'table')", exception.Message);
    }

    [Fact]
    public void GetColumns_InvalidDatabase_ThrowsArgumentException()
    {
        // ARRANGE
        var sqlServerObjects = new SqlServerObjects();
        var database = new SqlServerDatabase { Name = "TestDB" };
        var schema = new SqlServerSchema { Name = "dbo" };

        database.Schemas.Add(schema);
        sqlServerObjects.Databases.Add(database);

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentException>(() => sqlServerObjects.GetColumns("NonExistentTable", "dbo", "BogusDB"));
        Assert.Contains("Database 'BogusDB' not found", exception.Message);
    }

    [Fact]
    public void GetColumns_InvalidSchema_ThrowsArgumentException()
    {
        // ARRANGE
        var sqlServerObjects = new SqlServerObjects();
        var database = new SqlServerDatabase { Name = "TestDB" };
        var schema = new SqlServerSchema { Name = "dbo" };

        database.Schemas.Add(schema);
        sqlServerObjects.Databases.Add(database);

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentException>(() => sqlServerObjects.GetColumns("NonExistentTable", "blah", "TestDB"));
        Assert.Contains("Schema 'blah' not found in database 'TestDB'", exception.Message);
    }

    [Fact]
    public void GetColumns_InvalidTable_ThrowsArgumentException()
    {
        // ARRANGE
        var sqlServerObjects = new SqlServerObjects();
        var database = new SqlServerDatabase { Name = "TestDB" };
        var schema = new SqlServerSchema { Name = "dbo" };

        database.Schemas.Add(schema);
        sqlServerObjects.Databases.Add(database);

        // ACT & ASSERT
        var exception = Assert.Throws<ArgumentException>(() => sqlServerObjects.GetColumns("NonExistentTable", "dbo", "TestDB"));
        Assert.Contains("Table 'NonExistentTable' not found in schema 'dbo' of database 'TestDB'", exception.Message);
    }

    [Fact]
    public void GetColumns_ValidInput_ReturnsColumns()
    {
        // ARRANGE
        var sqlServerObjects = new SqlServerObjects();
        var database = new SqlServerDatabase { Name = "TestDB" };
        var schema = new SqlServerSchema { Name = "dbo" };
        var table = new SqlServerTable { Name = "TestTable" };
        var column1 = new SqlServerColumn { Name = "Column1" };
        var column2 = new SqlServerColumn { Name = "Column2" };

        table.Columns.Add(column1);
        table.Columns.Add(column2);
        schema.Tables.Add(table);
        database.Schemas.Add(schema);
        sqlServerObjects.Databases.Add(database);

        // ACT
        var columns = sqlServerObjects.GetColumns("TestTable", "dbo", "TestDB");

        // ASSERT
        Assert.NotNull(columns);
        Assert.Equal(2, columns.Count);
        Assert.Contains(columns, c => c.Name == "Column1");
        Assert.Contains(columns, c => c.Name == "Column2");
    }

}
