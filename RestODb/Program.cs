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

builder.Services.AddRestoDbJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

app.UseRestoDbLoggingMiddleware(builder.Configuration);

await app.MapRestoDbRoutesAsync(builder.Configuration);


app.UseRestoDbAuthentication(builder.Configuration);
app.UseRestoDbSwaggerUi(builder.Configuration);

await app.RunAsync();
