using KnightMoves.SqlObjects.ForSqlServer.Model;

namespace KnightMoves.SqlObjects.ForSqlServer.Tests.Model;

public class SqlObjectsForSqlServerStateTests
{
    [Fact]
    public void TryStart_FirstCall_ReturnsTrue()
    {
        // ARRANGE
        var state = new SqlObjectsForSqlServerState();

        // ACT
        var result = state.TryStart();

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void TryStart_SecondCall_ReturnsFalse()
    {
        // ARRANGE
        var state = new SqlObjectsForSqlServerState();

        state.TryStart();

        // ACT
        var result = state.TryStart();

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public void MarkCompleted_SetsHasCompletedToTrue()
    {
        // ARRANGE
        var state = new SqlObjectsForSqlServerState();

        // ACT
        state.MarkCompleted();

        // ASSERT
        Assert.True(state.HasCompleted);
    }
}
