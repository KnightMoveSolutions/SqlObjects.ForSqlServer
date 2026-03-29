namespace KnightMoves.SqlObjects.ForSqlServer.Model;

public class SqlServerColumn
{
    public string Name { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public bool IsPrimaryKey { get; set; }

    public int? PrimaryKeyOrdinal { get; set; }

    public bool IsForeignKey { get; set; }

    public string? ForeignKeyName { get; set; }

    public int OrdinalPosition { get; set; }

    public int? CharacterMaxLength { get; set; }

    public byte? NumericPrecision { get; set; }

    public int? NumericScale { get; set; }

    public int? DateTimePrecision { get; set; }

    public bool IsNullable { get; set; }

    public string? ColumnDefault { get; set; }

    public string? RefSchema { get; set; }

    public string? RefTable { get; set; }

    public string? RefColumn { get; set; }
}
