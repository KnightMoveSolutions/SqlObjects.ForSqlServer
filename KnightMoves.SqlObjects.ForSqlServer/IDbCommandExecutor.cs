using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer;

public interface IDbCommandExecutor
{
    Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken = default);
}
