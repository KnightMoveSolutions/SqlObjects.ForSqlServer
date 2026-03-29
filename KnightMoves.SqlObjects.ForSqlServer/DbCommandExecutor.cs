using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer;

public class DbCommandExecutor : IDbCommandExecutor
{
    public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken = default) => 
        command.ExecuteReaderAsync(cancellationToken);
}
