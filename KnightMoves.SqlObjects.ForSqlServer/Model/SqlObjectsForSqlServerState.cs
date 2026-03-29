namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public sealed class SqlObjectsForSqlServerState
{
    private int _started = 0;
    private int _completed = 0;

    public bool TryStart() => Interlocked.Exchange(ref _started, 1) == 0;
    public void MarkCompleted() => Interlocked.Exchange(ref _completed, 1);
    public bool HasCompleted => Volatile.Read(ref _completed) == 1;
}
