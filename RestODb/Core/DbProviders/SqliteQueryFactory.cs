using Dapper;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Data.SQLite;
using static Org.BouncyCastle.Math.EC.ECCurve;

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

    public override async Task<IEnumerable<ColumnDescription>> GetEntityColumnsDescription(string tableName)
    {
        var result = await GetFactory().Connection.ExecuteReaderAsync($"pragma table_info('{tableName}')");
        List<ColumnDescription> descriptions = new();
        while (result.Read())
        {
            string name = result.GetString(1);
            string type = result.GetString(2);
            bool notNull = result.GetBoolean(3);

            descriptions.Add(new() { Name = name, Type = type, NotNull = notNull });
        }
        return descriptions;
    }
}

public struct ColumnDescription
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool NotNull { get; set; }
}