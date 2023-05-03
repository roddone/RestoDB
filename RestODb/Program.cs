using Microsoft.AspNetCore.Mvc;
using SqlKata;
using SqlKata.Execution;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appSettings.json");
if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appSettings.Development.json");

//add correct SQLKata provider 
DbProviders provider = builder.Configuration.GetValue<DbProviders>("provider");
if (provider == DbProviders.NpgSql) builder.Services.AddSingleton<SqlKataQueryFactory, NpgSqlQueryFactory>();
else if (provider == DbProviders.SqlServer) builder.Services.AddSingleton<SqlKataQueryFactory, SqlServerQueryFactory>();

// enable swagger
bool swaggerEnabled = builder.Configuration.GetValue<bool>("enableSwagger");
if (swaggerEnabled) builder.Services.AddEndpointsApiExplorer().AddOpenApiDocument(c => { c.Title = "RestODb"; c.Version = "v1"; });

builder.Services.AddLogging();
var app = builder.Build();

//get available routes
var sqlKata = app.Services.GetService<SqlKataQueryFactory>() ?? throw new Exception("Cannot find service of type SqlKataQueryFactory");
var tables = await sqlKata.GetTablesListAsync();
var limitTo = builder.Configuration.GetSection("limitTo").Get<string[]>();
if (limitTo?.Any() == true) tables = tables.Where(t => limitTo.Contains(t));

//create routes (create individual routes so they can appear in swagger
foreach (var table in tables)
{
    app.MapGet($"/api/{table}", ([FromServices] SqlKataQueryFactory factory, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc) =>
    {
        Query sqlQuery = factory.Create(table);

        if (!string.IsNullOrWhiteSpace(select)) sqlQuery = sqlQuery.Select(SafeSplit(select));

        if (take.HasValue) sqlQuery = sqlQuery.Take(take.Value);

        if (skip.HasValue) sqlQuery = sqlQuery.Skip(skip.Value);

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            string[] parts = SafeSplit(orderBy);
            sqlQuery = orderByDesc.HasValue && orderByDesc.Value ? sqlQuery.OrderByDesc(parts) : sqlQuery.OrderBy(parts);
        }

        return sqlQuery.GetAsync();
    });
}


if (swaggerEnabled)
{
    app.UseOpenApi();
    app.UseSwaggerUi3();
}

await app.RunAsync();


static string[] SafeSplit(string input)
{
    return input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}