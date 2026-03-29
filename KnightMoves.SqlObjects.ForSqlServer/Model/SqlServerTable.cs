namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public class SqlServerTable
{
    public string Name { get; set; } = string.Empty;

    public bool IsView { get; set; }

    public List<SqlServerColumn> Columns { get; set; } = new();
}
