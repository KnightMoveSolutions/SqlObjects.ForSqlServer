using KnightMoves.SqlObjects.ForSqlServer.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KnightMoves.SqlObjects.ForSqlServer;

public static class HostExtensions
{
    public static IHost UseSqlServerObjectsForSqlServer(this IHost host) =>
        host.UseSqlServerObjectsForSqlServer(new CancellationToken()).GetAwaiter().GetResult();

    public static async Task<IHost> UseSqlServerObjectsForSqlServer(this IHost host, CancellationToken cancellationToken = default)
    {
        var state = host.Services.GetRequiredService<SqlObjectsForSqlServerState>();

        // Idempotency check
        if (!state.TryStart())
            return host;

        using var scope = host.Services.CreateScope();

        var sqlServerObjects = scope.ServiceProvider.GetRequiredService<SqlServerObjects>();

        var schemaLoaders = scope.ServiceProvider.GetServices<ISchemaLoader>();

        foreach (var schemaLoader in schemaLoaders)
            await schemaLoader.LoadSchemasAsync(sqlServerObjects, cancellationToken).ConfigureAwait(false);

        state.MarkCompleted();

        return host;
    }
}
