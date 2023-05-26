using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

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
        QueryFactory factory = GetFactory(_config.GetValue<string>("DbConnection"), log);

        return factory.Query(tableName);
    }

    protected abstract IDbConnection GetConnection(string connectionString);

    protected abstract Compiler GetCompiler();

    public abstract Task<IEnumerable<string>> GetTablesListAsync();
}
