namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public class SqlServerDatabase
{
    public string Name { get; set; } = string.Empty;

    public List<SqlServerSchema> Schemas { get; set; } = new();
}
