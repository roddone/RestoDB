
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
