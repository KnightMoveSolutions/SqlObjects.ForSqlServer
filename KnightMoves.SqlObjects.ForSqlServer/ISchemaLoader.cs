using KnightMoves.SqlObjects.ForSqlServer.Model;

namespace KnightMoves.SqlObjects.ForSqlServer;

public interface ISchemaLoader
{
    Task LoadSchemasAsync(SqlServerObjects sqlServerObjects, CancellationToken cancellationToken = default);
}
