namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public class SqlServerSchema
{
    public string Name { get; set; } = string.Empty;

    public List<SqlServerTable> Tables { get; set; } = new();
}
