using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

public class NpgSqlQueryFactory : SqlKataQueryFactory
{
    public NpgSqlQueryFactory(IConfiguration config, ILogger<SqlKataQueryFactory> logger) : base(config, logger)
    {
    }

    protected override Compiler GetCompiler() => new PostgresCompiler();

    protected override IDbConnection GetConnection(string connectionString)
        => new NpgsqlConnection(connectionString);

    public override async Task<IEnumerable<string>> GetTablesListAsync()
    {
        List<string> tables = new List<string>();
        tables.AddRange(await Create("pg_catalog.pg_tables")
                            .WhereNotIn("schemaname", new string[] { "pg_catalog", "information_schema" })
                            .Select("tablename")
                            .GetAsync<string>());

        tables.AddRange(await Create("pg_catalog.pg_views")
                            .WhereNotIn("schemaname", new string[] { "pg_catalog", "information_schema" })
                            .Select("viewname")
                            .GetAsync<string>());

        return tables;
    }

    public override async Task<IEnumerable<ColumnDescription>> GetEntityColumnsDescription(string tableName)
    {
        var result = (await Create("information_schema.columns")
            .Where("table_name", tableName)
            .Select("column_name as Name", "data_type as Type", "is_nullable as CanBeNull")
            .GetAsync()
            ).Select(r => new ColumnDescription
            {
                Name = r.Name,
                Type = r.Type,
                NotNull = r.CanBeNull == "NO"
            });

        return result;

    }
}
