namespace RestODb.Core
{
    public static class AppBuilderDbExtensions
    {
        public static IServiceCollection AddRestoDBDbProvider(this IServiceCollection services, IConfiguration config) 
        {
            DbProviders provider = config.GetValue<DbProviders>("DbProvider");
            return provider switch
            {
                DbProviders.NpgSql => services.AddSingleton<SqlKataQueryFactory, NpgSqlQueryFactory>(),
                DbProviders.SqlServer => services.AddSingleton<SqlKataQueryFactory, SqlServerQueryFactory>(),
                DbProviders.Sqlite => services.AddSingleton<SqlKataQueryFactory, SqliteQueryFactory>(),
                _ => throw new NotImplementedException("Unkwnown database provider")
            };
        }
    }
}
