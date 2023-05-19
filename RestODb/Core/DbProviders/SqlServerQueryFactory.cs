
using Microsoft.Data.SqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

public class SqlServerQueryFactory : SqlKataQueryFactory
{
    public SqlServerQueryFactory(IConfiguration config, ILogger<SqlKataQueryFactory> logger) : base(config, logger)
    {
    }

    protected override Compiler GetCompiler() => new SqlServerCompiler();

    protected override IDbConnection GetConnection(string connectionString)
        => new SqlConnection(connectionString);

    public override Task<IEnumerable<string>> GetTablesListAsync()
    => Create("INFORMATION_SCHEMA.TABLES")
        .WhereNot("TABLE_SCHEMA", "sys")
        .Select("TABLE_NAME")
        .GetAsync<string>();
}
