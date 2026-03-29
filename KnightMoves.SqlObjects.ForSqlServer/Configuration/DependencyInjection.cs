using KnightMoves.SqlObjects.ForSqlServer.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlObjectsForSqlServerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var options = SqlObjectsForSqlServerOptions.Create(configuration) ?? new();
        services.AddSingleton(options);
        return services.RegisterServices();
    }

    public static IServiceCollection AddSqlObjectsForSqlServerOptions(this IServiceCollection services, SqlObjectsForSqlServerOptions options)
    {
        services.AddSingleton(options);
        return services.RegisterServices();
    }

    public static IServiceCollection AddSqlObjectsForSqlServerOptions(this IServiceCollection services, Action<SqlObjectsForSqlServerOptions> configure)
    {
        var options = new SqlObjectsForSqlServerOptions();
        configure(options);
        services.AddSingleton(options);
        return services.RegisterServices();
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<SqlObjectsForSqlServerState>();
        services.AddTransient<IDbCommandExecutor, DbCommandExecutor>();
        services.AddSingleton<SqlServerObjects>();
        services.AddSingleton<ISchemaLoader, DefaultSchemaLoader>();

        // Factory functions
        services.AddTransient<Func<string, DbConnection>>( _ => (string connStr) => new SqlConnection(connStr));
        services.AddTransient<Func<string, DbConnection, IDbCommand>>
        ( _ => 
            (string sql, DbConnection conn) =>
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                return cmd;
            }
        );

        return services;
    }
}
