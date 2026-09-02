using KnightMoves.SqlObjects.ForSqlServer.Configuration;
using KnightMoves.SqlObjects.ForSqlServer.Model;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace KnightMoves.SqlObjects.ForSqlServer;

public sealed class DefaultSchemaLoader(
    SqlObjectsForSqlServerOptions options,
    Func<string, DbConnection> connectionFactory,
    Func<string, DbConnection, IDbCommand> commandFactory,
    IDbCommandExecutor dbCommandExecutor
    ) : ISchemaLoader
{
    private readonly SqlObjectsForSqlServerOptions _options = options;
    private readonly Func<string, DbConnection> _connectionFactory = connectionFactory;
    private readonly Func<string, DbConnection, IDbCommand> _commandFactory = commandFactory;
    private readonly IDbCommandExecutor _dbCommandExecutor = dbCommandExecutor;

    public async Task LoadSchemasAsync(SqlServerObjects sqlServerObjects, CancellationToken cancellationToken = default)
    {
        foreach (var dbConfig in _options.Databases.Keys)
            await LoadSqlObjects(_options.Databases[dbConfig], sqlServerObjects, cancellationToken);
    }

    private static string GetSchemaSql(List<string> schemas) => 
        TSQL
            .SELECT()
              .COLUMN("SCHEMA_NAME")
            .FROM(schema: "INFORMATION_SCHEMA", "SCHEMATA")
            .WHERE()
                .COLUMN("SCHEMA_NAME").IN("@Schemas")
            .ORDERBY()
              .COLUMN("SCHEMA_NAME").ASC()
            .Build(new { Schemas = schemas })
        ;

    private static string GetTableSql(string schema) =>
        TSQL
            .SELECT()
              .COLUMN("TABLE_SCHEMA")
              .COLUMN("TABLE_NAME")
            .FROM(schema: "INFORMATION_SCHEMA", "TABLES")
            .WHERE("TABLE_TYPE").IsEqualTo("BASE TABLE").AND()
                .COLUMN("TABLE_SCHEMA").IsEqualTo("@Schema").AND()
                .Literal("TABLE_NAME NOT LIKE 'sys%'")
            .ORDERBY()
              .COLUMN("TABLE_SCHEMA").ASC()
              .COLUMN("TABLE_NAME").ASC()
            .Build(new { Schema = schema })
        ;

    private static string GetViewSql(string schema) =>
        TSQL
            .SELECT()
              .COLUMN("TABLE_SCHEMA")
              .COLUMN("TABLE_NAME")
            .FROM(schema: "INFORMATION_SCHEMA", "TABLES")
            .WHERE("TABLE_TYPE").IsEqualTo("VIEW").AND()
                .COLUMN("TABLE_SCHEMA").IsEqualTo("@Schema")
            .ORDERBY()
              .COLUMN("TABLE_SCHEMA").ASC()
              .COLUMN("TABLE_NAME").ASC()
            .Build(new { Schema = schema })
        ;

    private static string GetColumnSql(string schema, string table)
    {
        var pkSql = TSQL
            .SELECT()
                .COLUMN("k", "TABLE_SCHEMA")
                .COLUMN("k", "TABLE_NAME")
                .COLUMN("k", "COLUMN_NAME")
                .COLUMN("k", "ORDINAL_POSITION").AS("key_ordinal")
            .FROM("INFORMATION_SCHEMA", "TABLE_CONSTRAINTS", "t")
            .INNERJOIN("INFORMATION_SCHEMA", "KEY_COLUMN_USAGE", "k")
                .ON("t", "CONSTRAINT_NAME").IsEqualTo("k", "CONSTRAINT_NAME")
                .AND("t", "CONSTRAINT_SCHEMA").IsEqualTo("k", "CONSTRAINT_SCHEMA")
            .WHERE("t", "CONSTRAINT_TYPE").IsEqualTo("PRIMARY KEY")
            .Build()
        ;

        var fkSql = TSQL
                .SELECT()
                    .COLUMN("k", "TABLE_SCHEMA")
                    .COLUMN("k", "TABLE_NAME")
                    .COLUMN("k", "COLUMN_NAME")
                    .COLUMN("t", "CONSTRAINT_NAME").AS("fk_name")
                    .COLUMN("ccu", "TABLE_SCHEMA").AS("referenced_table_schema")
                    .COLUMN("ccu", "TABLE_NAME").AS("referenced_table_name")
                    .COLUMN("ccu", "COLUMN_NAME").AS("referenced_column_name")
                .FROM("INFORMATION_SCHEMA", "TABLE_CONSTRAINTS", "t")
                .INNERJOIN("INFORMATION_SCHEMA", "KEY_COLUMN_USAGE", "k")
                    .ON("t", "CONSTRAINT_NAME").IsEqualTo("k", "CONSTRAINT_NAME")
                    .AND("t", "CONSTRAINT_SCHEMA").IsEqualTo("k", "CONSTRAINT_SCHEMA")
                .INNERJOIN("INFORMATION_SCHEMA", "REFERENTIAL_CONSTRAINTS", "rc")
                    .ON("t", "CONSTRAINT_NAME").IsEqualTo("rc", "CONSTRAINT_NAME")
                    .AND("t", "CONSTRAINT_SCHEMA").IsEqualTo("rc", "CONSTRAINT_SCHEMA")
                .INNERJOIN("INFORMATION_SCHEMA", "CONSTRAINT_COLUMN_USAGE", "ccu")
                    .ON("rc", "UNIQUE_CONSTRAINT_NAME").IsEqualTo("ccu", "CONSTRAINT_NAME")
                    .AND("rc", "UNIQUE_CONSTRAINT_SCHEMA").IsEqualTo("ccu", "CONSTRAINT_SCHEMA")
                .WHERE("t", "CONSTRAINT_TYPE").IsEqualTo("FOREIGN KEY")
                .Build()
            ;

        var existsSubQuery = TSQL
            .SELECT()
                .Literal("1")
            .FROM("INFORMATION_SCHEMA", "TABLES", "t")
            .WHERE("t", "TABLE_SCHEMA").IsEqualTo("c", "TABLE_SCHEMA")
                .AND("t", "TABLE_NAME").IsEqualTo("c", "TABLE_NAME")
                .AND("t", "TABLE_TYPE").IN("BASE TABLE", "VIEW")
        ;

        var colSql = TSQL
            .Script(@$"
                WITH 
                pk AS (
                    {pkSql}
                ),
                fk AS (
                    {fkSql}
                )
            ")
            .SELECT()
                .COLUMN("c", "TABLE_SCHEMA")
                .COLUMN("c", "TABLE_NAME")
                .COLUMN("c", "COLUMN_NAME")
                .COLUMN("c", "ORDINAL_POSITION")
                .COLUMN("c", "DATA_TYPE")
                .COLUMN("c", "CHARACTER_MAXIMUM_LENGTH")
                .COLUMN("c", "NUMERIC_PRECISION")
                .COLUMN("c", "NUMERIC_SCALE")
                .COLUMN("c", "DATETIME_PRECISION")
                .COLUMN("c", "IS_NULLABLE")
                .COLUMN("c", "COLUMN_DEFAULT")
                .CASE()
                    .WHEN("pk", "COLUMN_NAME")
                        .IS_NOT_NULL()
                    .THEN(1)
                    .ELSE(0)
                .END().AS("is_primary_key")
                .COLUMN("pk", "key_ordinal")
                .CASE()
                    .WHEN("fk", "COLUMN_NAME")
                        .IS_NOT_NULL()
                    .THEN(1)
                    .ELSE(0)
                .END().AS("is_foreign_key")
                .COLUMN("fk", "fk_name")
                .COLUMN("fk", "referenced_table_schema")
                .COLUMN("fk", "referenced_table_name")
                .COLUMN("fk", "referenced_column_name")
            .FROM("INFORMATION_SCHEMA", "COLUMNS", "c")
            .LEFTJOIN("pk")
                .ON("pk", "TABLE_SCHEMA").IsEqualTo("c", "TABLE_SCHEMA")
                .AND("pk", "TABLE_NAME").IsEqualTo("c", "TABLE_NAME")
                .AND("pk", "COLUMN_NAME").IsEqualTo("c", "COLUMN_NAME")
            .LEFTJOIN("fk")
                .ON("fk", "TABLE_SCHEMA").IsEqualTo("c", "TABLE_SCHEMA")
                .AND("fk", "TABLE_NAME").IsEqualTo("c", "TABLE_NAME")
                .AND("fk", "COLUMN_NAME").IsEqualTo("c", "COLUMN_NAME")
            .WHERE()
                .EXISTS(existsSubQuery)
                .AND().COLUMN("c", "TABLE_SCHEMA").IsEqualTo("@Schema").AND()
                .AND().COLUMN("c", "TABLE_NAME").IsEqualTo("@Table")
                .ORDERBY()
                    .COLUMN("c", "TABLE_SCHEMA").ASC()
                    .COLUMN("c", "TABLE_NAME").ASC()
                    .COLUMN("c", "ORDINAL_POSITION").ASC()
            .Build(new { Schema = schema, Table = table })
        ;

        return colSql;
    }

    private async Task LoadSqlObjects(DatabaseConfig dbConfig, SqlServerObjects sqlServerObjects, CancellationToken cancellationToken)
    {
        var sqlConnBuilder = new SqlConnectionStringBuilder(dbConfig.ConnectionString);

        var sqlServerDB = new SqlServerDatabase { Name = sqlConnBuilder.InitialCatalog };

        sqlServerObjects.Databases.Add(sqlServerDB);

        using var connection = _connectionFactory(dbConfig.ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await LoadSchemas(connection, sqlServerDB, dbConfig.Schemas, cancellationToken).ConfigureAwait(false);

        return;
    }

    private async Task LoadSchemas(DbConnection connection, SqlServerDatabase sqlServerDb, List<string> schemas, CancellationToken cancellationToken)
    {
        var schemaSql = DefaultSchemaLoader.GetSchemaSql(schemas);
        
        using (var cmd = _commandFactory(schemaSql, connection) as DbCommand)
        {
            if (cmd == null)
            {
                ThrowDbCommandException();
                return;
            }

            using var reader = await _dbCommandExecutor.ExecuteReaderAsync(cmd, cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var schemaname = reader.GetOrdinal("SCHEMA_NAME");
                var sqlServerSchema = new SqlServerSchema { Name = reader.GetString(schemaname) };
                sqlServerDb.Schemas.Add(sqlServerSchema);
            }
        }

        foreach (var sqlServerSchema in sqlServerDb.Schemas)
        {
            await LoadTables(connection, sqlServerSchema, cancellationToken).ConfigureAwait(false);
            await LoadViews(connection, sqlServerSchema, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task LoadTables(DbConnection connection, SqlServerSchema sqlServerSchema, CancellationToken cancellationToken)
    {
        var tableSql = GetTableSql(sqlServerSchema.Name);

        using (var cmd = _commandFactory(tableSql, connection) as DbCommand)
        {
            if (cmd == null)
            {
                ThrowDbCommandException();
                return;
            }
            using var reader = await _dbCommandExecutor.ExecuteReaderAsync(cmd, cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var sqlServerTable = new SqlServerTable { Name = reader.GetString("TABLE_NAME"), IsView = false };
                sqlServerSchema.Tables.Add(sqlServerTable);
            }
        }
        foreach (var sqlServerTable in sqlServerSchema.Tables.Where(t => !t.IsView))
            await LoadColumns(connection, sqlServerSchema, sqlServerTable, cancellationToken).ConfigureAwait(false);
    }

    private async Task LoadViews(DbConnection connection, SqlServerSchema sqlServerSchema, CancellationToken cancellationToken)
    {
        var viewSql = DefaultSchemaLoader.GetViewSql(sqlServerSchema.Name);

        using (var cmd = _commandFactory(viewSql, connection) as DbCommand)
        {
            if (cmd == null)
            {
                ThrowDbCommandException();
                return;
            }
            using var reader = await _dbCommandExecutor.ExecuteReaderAsync(cmd, cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var sqlServerView = new SqlServerTable { Name = reader.GetString("TABLE_NAME"), IsView = true };
                sqlServerSchema.Tables.Add(sqlServerView);
            }
        }

        foreach (var sqlServerView in sqlServerSchema.Tables.Where(t => t.IsView))
            await LoadColumns(connection, sqlServerSchema, sqlServerView, cancellationToken).ConfigureAwait(false);
    }

    private async Task LoadColumns(DbConnection connection, SqlServerSchema sqlServerSchema, SqlServerTable sqlServerTable, CancellationToken cancellationToken)
    {
        var columnSql = GetColumnSql(sqlServerSchema.Name, sqlServerTable.Name);

        using (var cmd = _commandFactory(columnSql, connection) as DbCommand)
        {
            if (cmd == null)
            {
                ThrowDbCommandException();
                return;
            }
            using (var reader = await _dbCommandExecutor.ExecuteReaderAsync(cmd, cancellationToken).ConfigureAwait(false))
            {
                var colname = reader.GetOrdinal("COLUMN_NAME");
                var ordpos = reader.GetOrdinal("ORDINAL_POSITION");
                var datatype = reader.GetOrdinal("DATA_TYPE");
                var charmaxlen = reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH");
                var numprec = reader.GetOrdinal("NUMERIC_PRECISION");
                var numscale = reader.GetOrdinal("NUMERIC_SCALE");
                var dtprec = reader.GetOrdinal("DATETIME_PRECISION");
                var isnullable = reader.GetOrdinal("IS_NULLABLE");
                var coldef = reader.GetOrdinal("COLUMN_DEFAULT");
                var ispk = reader.GetOrdinal("is_primary_key");
                var pkord = reader.GetOrdinal("key_ordinal");
                var isfk = reader.GetOrdinal("is_foreign_key");
                var fkname = reader.GetOrdinal("fk_name");
                var refschema = reader.GetOrdinal("referenced_table_schema");
                var reftable = reader.GetOrdinal("referenced_table_name");
                var refcolumn = reader.GetOrdinal("referenced_column_name");

                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dt = reader.GetString(datatype);

                    var column = new SqlServerColumn
                    {
                        Name = reader.GetString(colname),
                        OrdinalPosition = reader.GetInt32(ordpos),
                        DataType = dt,
                        CharacterMaxLength = reader.IsDBNull(charmaxlen) ? null : reader.GetInt32(charmaxlen),
                        NumericPrecision = reader.IsDBNull(numprec) ? null : reader.GetByte(numprec),
                        NumericScale = reader.IsDBNull(numscale) ? null : reader.GetInt32(numscale),
                        DateTimePrecision = reader.IsDBNull(dtprec) ? null : reader.GetInt16(dtprec),
                        IsNullable = reader.GetString(isnullable) == "YES",
                        ColumnDefault = reader.IsDBNull(coldef) ? null : reader.GetString(coldef),
                        IsPrimaryKey = reader.GetInt32(ispk) == 1,
                        PrimaryKeyOrdinal = reader.IsDBNull(pkord) ? null : reader.GetInt32(pkord),
                        IsForeignKey = reader.GetInt32(isfk) == 1,
                        ForeignKeyName = reader.IsDBNull(fkname) ? null : reader.GetString(fkname),
                        RefSchema = reader.IsDBNull(refschema) ? null : reader.GetString(refschema),
                        RefTable = reader.IsDBNull(reftable) ? null : reader.GetString(reftable),
                        RefColumn = reader.IsDBNull(refcolumn) ? null : reader.GetString(refcolumn),
                    };

                    sqlServerTable.Columns.Add(column);
                }
            }
        }
    }

    private static void ThrowDbCommandException()
    {
        throw new InvalidOperationException($"Command factory {typeof(Func<string, DbConnection, IDbCommand>)} must return a type that derives from {typeof(DbCommand)}.");
    }
}
