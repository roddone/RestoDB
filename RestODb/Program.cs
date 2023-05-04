using Access.It.Web.Swagger.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RestODb.Core;
using SqlKata;
using SqlKata.Execution;
using System.Diagnostics;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appSettings.json");
if (builder.Environment.IsDevelopment()) builder.Configuration.AddJsonFile("appSettings.Development.json");

//add correct SQLKata provider 
DbProviders provider = builder.Configuration.GetValue<DbProviders>("DbProvider");
if (provider == DbProviders.NpgSql) builder.Services.AddSingleton<SqlKataQueryFactory, NpgSqlQueryFactory>();
else if (provider == DbProviders.SqlServer) builder.Services.AddSingleton<SqlKataQueryFactory, SqlServerQueryFactory>();

builder.Services.AddCustomSwagger(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// handle authentication
bool authEnabled = builder.Configuration.GetValue<bool>("Auth:Enabled");
if (authEnabled) builder.Services.AddJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

//get available routes
var sqlKata = app.Services.GetService<SqlKataQueryFactory>() ?? throw new Exception("Cannot find service of type SqlKataQueryFactory");
var tables = await sqlKata.GetTablesListAsync();
var limitTo = builder.Configuration.GetSection("limitTo").Get<string[]>();
if (limitTo?.Any() == true) tables = tables.Where(t => limitTo.Contains(t));

string apiSegment = builder.Configuration.GetValue("ApiSegment", "api")!;

app.Use(async (ctx, next) =>
{
    //only /api/* routes
    if (!ctx.Request.Path.Value?.StartsWith($"/{apiSegment}") == true)
    {
        await next(ctx);
        return;
    }

    var logger = ctx.RequestServices.GetService<ILogger<Program>>()!;
    logger.LogDebug("Start selecting rows in table '{table}', with params : select={select}; skip={skip}; take={take}; orderBy={orderBy}; orderByDesc={orderByDesc};", ctx.Request.Path.Value.Replace($"/{apiSegment}/", string.Empty), ctx.Request.Query["select"], ctx.Request.Query["skip"], ctx.Request.Query["take"], ctx.Request.Query["orderBy"], ctx.Request.Query["orderByDesc"]);

    Stopwatch sw = Stopwatch.StartNew();
    try
    {
        await next(ctx);
        logger.LogDebug("Request finished, elapsed: {elapsed}", sw.Elapsed);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unable to process request, elapsed: {elapsed}", sw.Elapsed);
        throw;
    }
});

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

app.UseCustomSwaggerUi(builder.Configuration);

await app.RunAsync();


static string[] SafeSplit(string input)
{
    return input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}