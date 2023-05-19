
using Microsoft.Data.SqlClient;
using Npgsql;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SQLite;

public abstract class SqlKataQueryFactory
{
    private readonly IConfiguration _config;
    private readonly ILogger<SqlKataQueryFactory> _logger;
    private static readonly Action<SqlResult> _emptyLogger = (compiled) => { };

    public SqlKataQueryFactory(IConfiguration config, ILogger<SqlKataQueryFactory> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected QueryFactory GetFactory(string connectionString, bool log = false)
    {
        IDbConnection connection = GetConnection(connectionString);
        Compiler compiler = GetCompiler();

        return new QueryFactory(connection, compiler)
        {
            Logger = log ? compiled => _logger.LogDebug(compiled.ToString()) : _emptyLogger
        };
    }

    public Query Create(string tableName, bool log = false)
    {
        QueryFactory factory = GetFactory(_config.GetConnectionString("RestoDB"), log);

        return factory.Query(tableName);
    }

    protected abstract IDbConnection GetConnection(string connectionString);

    protected abstract Compiler GetCompiler();

    public abstract Task<IEnumerable<string>> GetTablesListAsync();
}

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