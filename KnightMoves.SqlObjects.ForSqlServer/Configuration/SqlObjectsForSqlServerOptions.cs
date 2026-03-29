using Microsoft.Extensions.Configuration;

namespace KnightMoves.SqlObjects.ForSqlServer.Configuration;

public class SqlObjectsForSqlServerOptions
{
    public Dictionary<string, DatabaseConfig> Databases { get; set; } = new();

    public SqlObjectsForSqlServerOptions() { }

    public static SqlObjectsForSqlServerOptions Create(IConfiguration config)
    {
        var options = config.GetSection(nameof(SqlObjectsForSqlServerOptions))
                            .Get<SqlObjectsForSqlServerOptions>();

        if (options == null)
            return new();

        options.Databases.ToList().ForEach(kvp =>
        {
            var connStr = config.GetConnectionString(kvp.Key);

            if (!string.IsNullOrEmpty(connStr))
                kvp.Value.ConnectionString = connStr;

            if (kvp.Value.Schemas == null || !kvp.Value.Schemas.Any())
                kvp.Value.Schemas = new List<string> { "dbo" };
        });

        return options;
    }
}
