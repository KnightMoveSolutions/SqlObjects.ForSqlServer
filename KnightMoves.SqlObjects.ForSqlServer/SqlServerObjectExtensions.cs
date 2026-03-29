using KnightMoves.SqlObjects.ForSqlServer.Model;

namespace KnightMoves.SqlObjects.ForSqlServer;

public static class SqlServerObjectExtensions
{
    public static List<SqlServerColumn> ForSelect(this List<SqlServerColumn> columns)
    {
        if (columns.Count == 0)
            return columns;

        return columns.OrderBy(c => c.OrdinalPosition).ToList();
    }

    public static List<SqlServerColumn> ForInsert(this List<SqlServerColumn> columns) =>
        columns.ForSelect().Where(c => !c.IsPrimaryKey).ToList();

    public static List<SqlServerColumn> ForUpdate(this List<SqlServerColumn> columns) =>
        ForInsert(columns);

    public static List<string> ToColumnNames(this List<SqlServerColumn> columns) =>
        columns.Select(c => c.Name).ToList();
}
