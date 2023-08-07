using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SQLite;

public class MysqlQueryFactory : SqlKataQueryFactory
{
    public MysqlQueryFactory(IConfiguration config, ILogger<SqlKataQueryFactory> logger) : base(config, logger)
    {
    }

    public override Task<IEnumerable<string>> GetTablesListAsync()
        => Create("information_schema.TABLES")
            .WhereIn("TABLE_TYPE", new string[] { "BASE TABLE", "VIEW" })
            .Select("TABLE_NAME")
            .GetAsync<string>();

    protected override Compiler GetCompiler()
    => new MySqlCompiler();

    protected override IDbConnection GetConnection(string connectionString)
    => new MySqlConnection(connectionString);
}