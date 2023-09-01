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

    protected QueryFactory GetFactory(bool log = false)
    {
        IDbConnection connection = GetConnection(_config.GetValue<string>("DbConnection"));
        Compiler compiler = GetCompiler();

        return new QueryFactory(connection, compiler)
        {
            Logger = log ? compiled => _logger.LogDebug(compiled.ToString()) : _emptyLogger
        };
    }

    public Query Create(string? tableName = null, bool log = false)
    {
        QueryFactory factory = GetFactory(log);

        return factory.Query(tableName);
    }

    protected abstract IDbConnection GetConnection(string connectionString);

    protected abstract Compiler GetCompiler();

    public abstract Task<IEnumerable<string>> GetTablesListAsync();

    public virtual Task<IEnumerable<ColumnDescription>> GetEntityColumnsDescription(string tableName)
    {
        return Task.FromResult(Array.Empty<ColumnDescription>().AsEnumerable());
    }
}
