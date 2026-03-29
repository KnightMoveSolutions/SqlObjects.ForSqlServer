namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public class SqlServerObjects
{
    public List<SqlServerDatabase> Databases { get; set; } = new();

    public List<SqlServerColumn> GetColumns(string table, string? schema = null, string? database = null)
    {
        if (string.IsNullOrEmpty(table))
            throw new ArgumentNullException(nameof(table));

        var db = string.IsNullOrEmpty(database) ? Databases.FirstOrDefault() : Databases.FirstOrDefault(d => d.Name.Equals(database, StringComparison.OrdinalIgnoreCase));

        if (db == null)
            throw new ArgumentException($"Database '{database}' not found.", nameof(database));

        var sch = string.IsNullOrEmpty(schema) ? db.Schemas.FirstOrDefault() : db.Schemas.FirstOrDefault(s => s.Name.Equals(schema, StringComparison.OrdinalIgnoreCase));

        if (sch == null)
            throw new ArgumentException($"Schema '{schema}' not found in database '{db.Name}'.", nameof(schema));

        var tbl = sch.Tables.FirstOrDefault(t => t.Name.Equals(table, StringComparison.OrdinalIgnoreCase));

        if (tbl == null)
            throw new ArgumentException($"Table '{table}' not found in schema '{sch.Name}' of database '{db.Name}'.", nameof(table));

        return tbl.Columns;
    }
}
