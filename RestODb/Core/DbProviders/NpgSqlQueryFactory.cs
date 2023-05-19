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
}
