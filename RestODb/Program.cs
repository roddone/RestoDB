using Access.It.Web.Swagger.Extensions;
using Microsoft.AspNetCore.Mvc;
using RestODb.Core;
using SqlKata;
using SqlKata.Execution;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appSettings.json");
if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appSettings.Development.json");

//add correct SQLKata provider 
DbProviders provider = builder.Configuration.GetValue<DbProviders>("DbProvider");
if (provider == DbProviders.NpgSql) builder.Services.AddSingleton<SqlKataQueryFactory, NpgSqlQueryFactory>();
else if (provider == DbProviders.SqlServer) builder.Services.AddSingleton<SqlKataQueryFactory, SqlServerQueryFactory>();

builder.Services.AddRestoDbSwagger(builder.Configuration);

// handle authentication
bool authEnabled = builder.Configuration.GetValue<bool>("Auth:Enabled");
if (authEnabled) builder.Services.AddRestoDbJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

//get available routes
var sqlKata = app.Services.GetService<SqlKataQueryFactory>() ?? throw new Exception("Cannot find service of type SqlKataQueryFactory");
var tables = await sqlKata.GetTablesListAsync();
var limitTo = builder.Configuration.GetSection("limitTo").Get<string[]>();
if (limitTo?.Any() == true) tables = tables.Where(t => limitTo.Contains(t));

string apiSegment = builder.Configuration.GetValue("ApiSegment", "api")!;
app.UseRestoDbLoggingMiddleware(apiSegment);

//create routes (create individual routes so they can appear in swagger
foreach (var table in tables)
{
    var route = app.MapGet($"/{apiSegment}/{table}", ([FromServices] SqlKataQueryFactory factory, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc) =>
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
    }).WithName(table).WithOpenApi();

    if (authEnabled) route.RequireAuthorization();
}

if (authEnabled)
{
    app
        .UseAuthentication()
        .UseAuthorization();
}

app.UseRestoDbSwaggerUi(builder.Configuration);

await app.RunAsync();


static string[] SafeSplit(string input)
{
    return input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}