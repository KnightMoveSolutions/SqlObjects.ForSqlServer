using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KnightMoves.SqlObjects.ForSqlServer;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> UseSqlServerObjectsForSqlServerAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app.Services.GetRequiredService<IHost>().UseSqlServerObjectsForSqlServer(cancellationToken);
        return app;
    }

    public static WebApplication UseSqlServerObjectsForSqlServer(this WebApplication app)
    {
        app.Services.GetRequiredService<IHost>().UseSqlServerObjectsForSqlServer();
        return app;
    }
}
