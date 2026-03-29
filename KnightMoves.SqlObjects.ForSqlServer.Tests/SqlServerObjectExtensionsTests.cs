using KnightMoves.SqlObjects.ForSqlServer.Model;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests;

public class SqlServerObjectExtensionsTests
{
    [Fact]
    public void ForSelect_DoesNothingWhenColumnsEmpty()
    {
        // ARRANGE
        var columns = new List<SqlServerColumn>();

        // ACT
        var orderedColumns = columns.ForSelect();

        // ASSERT
        Assert.Empty(orderedColumns);
    }

    [Fact]
    public void ForSelect_OrdersColumnsByOrdinalPosition()
    {
        // ARRANGE
        var columns = new List<SqlServerColumn>
        {
            new SqlServerColumn { Name = "ColumnB", OrdinalPosition = 2 },
            new SqlServerColumn { Name = "ColumnA", OrdinalPosition = 1 },
            new SqlServerColumn { Name = "ColumnC", OrdinalPosition = 3 }
        };

        // ACT
        var orderedColumns = columns.ForSelect();

        // ASSERT
        Assert.Equal("ColumnA", orderedColumns[0].Name);
        Assert.Equal("ColumnB", orderedColumns[1].Name);
        Assert.Equal("ColumnC", orderedColumns[2].Name);
    }

    [Fact]
    public void ForInsert_ReturnsNonPrimaryKeyColumns()
    {
        // ARRANGE
        var columns = new List<SqlServerColumn>
        {
            new SqlServerColumn { Name = "ColumnB", OrdinalPosition = 2 },
            new SqlServerColumn { Name = "ColumnA", OrdinalPosition = 1, IsPrimaryKey = true },
            new SqlServerColumn { Name = "ColumnC", OrdinalPosition = 3 }
        };

        // ACT
        var insertColumns = columns.ForInsert();

        // ASSERT
        Assert.Equal(2, insertColumns.Count);
        Assert.Equal("ColumnB", insertColumns[0].Name);
        Assert.Equal("ColumnC", insertColumns[1].Name);
    }

    [Fact]
    public void ForUpdate_ReturnsNonPrimaryKeyColumns()
    {
        // ARRANGE
        var columns = new List<SqlServerColumn>
        {
            new SqlServerColumn { Name = "ColumnB", OrdinalPosition = 2 },
            new SqlServerColumn { Name = "ColumnA", OrdinalPosition = 1, IsPrimaryKey = true },
            new SqlServerColumn { Name = "ColumnC", OrdinalPosition = 3 }
        };

        // ACT
        var updateColumns = columns.ForUpdate();

        // ASSERT
        Assert.Equal(2, updateColumns.Count);
        Assert.Equal("ColumnB", updateColumns[0].Name);
        Assert.Equal("ColumnC", updateColumns[1].Name);
    }

    [Fact]
    public void ForColumns_ReturnsColumnNamesCollection()
    {
        // ARRANGE
        var columns = new List<SqlServerColumn>
        {
            new SqlServerColumn { Name = "ColumnB", OrdinalPosition = 2 },
            new SqlServerColumn { Name = "ColumnA", OrdinalPosition = 1 },
            new SqlServerColumn { Name = "ColumnC", OrdinalPosition = 3 }
        };

        // ACT
        var columnNames = columns.ToColumnNames();

        // ASSERT
        Assert.Equal(3, columnNames.Count);
        Assert.Equal("ColumnB", columnNames[0]);
        Assert.Equal("ColumnA", columnNames[1]);
        Assert.Equal("ColumnC", columnNames[2]);
    }
}
