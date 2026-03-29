using Microsoft.Extensions.Configuration;

namespace KnightMoves.SqlObjects.ForSqlServer.Configuration;

public static class ConfigurationExtensions
{
    public static SqlObjectsForSqlServerOptions GetSqlObjectsForSqlServerOptions(this IConfiguration configuration) =>
            SqlObjectsForSqlServerOptions.Create(configuration);

    public static SqlObjectsForSqlServerOptions GetSqlObjectsForSqlServerOptions(this IConfiguration configuration, string section) =>
            SqlObjectsForSqlServerOptions.Create(configuration.GetSection(section));
}
