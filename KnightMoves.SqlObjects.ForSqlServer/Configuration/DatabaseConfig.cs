namespace KnightMoves.SqlObjects.ForSqlServer.Configuration;

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;

    public List<string> Schemas { get; set; } = new();
}
