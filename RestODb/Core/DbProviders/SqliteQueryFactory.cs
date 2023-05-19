using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SQLite;

public class SqliteQueryFactory : SqlKataQueryFactory
{
    public SqliteQueryFactory(IConfiguration config, ILogger<SqlKataQueryFactory> logger) : base(config, logger)
    {
    }

    public override Task<IEnumerable<string>> GetTablesListAsync()
        => Create("sqlite_master")
            .WhereIn("type", new string[] { "table", "view" })
            .Select("name")
            .GetAsync<string>();

    protected override Compiler GetCompiler()
    => new SqliteCompiler();

    protected override IDbConnection GetConnection(string connectionString)
    => new SQLiteConnection(connectionString);
}