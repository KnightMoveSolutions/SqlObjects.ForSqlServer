using KnightMoves.SqlObjects.ForSqlServer.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests;

public class HostExtensionsTests
{
    [Fact]
    public void UseSqlServerObjectsForSqlServer_Should_Be_Idempotent()
    {
        // ARRANGE
        var state = new SqlObjectsForSqlServerState();
        var sqlServerObjects = new SqlServerObjects();
        var mockSchemaLoader = new Mock<ISchemaLoader>();

        var hostBuilder = new HostBuilder();

        hostBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(_ => state);
            services.AddSingleton(_ => sqlServerObjects);
            services.AddSingleton(_ => mockSchemaLoader.Object);
        });

        var host = hostBuilder.Build();

        // ACT & ASSERT
        Assert.False(state.HasCompleted);

        host = host.UseSqlServerObjectsForSqlServer();

        Assert.True(state.HasCompleted);

        host = host.UseSqlServerObjectsForSqlServer(); // Call again to test idempotency

        mockSchemaLoader.Verify(s => s.LoadSchemasAsync(It.IsAny<SqlServerObjects>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(state.HasCompleted);
    }
}
