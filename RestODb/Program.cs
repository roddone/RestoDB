using Microsoft.AspNetCore.Mvc;
using SqlKata;
using SqlKata.Execution;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appSettings.json");
if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appSettings.Development.json");

bool swaggerEnabled = builder.Configuration.GetValue<bool>("enableSwagger");
DbProviders provider = builder.Configuration.GetValue<DbProviders>("provider");

if (provider == DbProviders.NpgSql) builder.Services.AddSingleton<SqlKataQueryFactory, NpgSqlQueryFactory>();
else if (provider == DbProviders.SqlServer) builder.Services.AddSingleton<SqlKataQueryFactory, SqlServerQueryFactory>();

if (swaggerEnabled) builder.Services.AddEndpointsApiExplorer().AddOpenApiDocument(c => { c.Title = "toto"; c.Version = "v1"; });

builder.Services.AddLogging();
var app = builder.Build();

//récupération des routes possibles
var tables = await GetTablesListAsync(provider, app.Services.GetService<SqlKataQueryFactory>());

var limitTo = builder.Configuration.GetSection("limitTo").Get<string[]>();
if (limitTo?.Any() == true) tables = tables.Where(t => limitTo.Contains(t));

foreach (var table in tables)
{
    app.MapGet($"/{table}", ([FromServices] SqlKataQueryFactory factory) =>
    {
        Query query = factory.Create(table);

        return query.GetAsync();
    });
}

if (swaggerEnabled)
{
    app.UseOpenApi();
    app.UseSwaggerUi3();
}

await app.RunAsync();


static async Task<IEnumerable<string>> GetTablesListAsync(DbProviders provider, SqlKataQueryFactory factory)
{
    if (provider == DbProviders.NpgSql)
        return await factory.Create("pg_catalog.pg_tables")
                            .WhereNotIn("schemaname", new string[] { "pg_catalog", "information_schema" })
                            .Select("tablename")
                            .GetAsync<string>();
    else return await factory.Create("INFORMATION_SCHEMA.TABLES")
                            .WhereNot("TABLE_SCHEMA", "sys")
                            .Select("TABLE_NAME")
                            .GetAsync<string>();
}