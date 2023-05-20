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

builder.Services.AddRestoDBCors(builder.Configuration);
builder.Services.AddRestoDBDbProvider(builder.Configuration);
builder.Services.AddRestoDBIpRateLimiter(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddRestoDbSwagger(builder.Configuration);
builder.Services.AddRestoDbJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

app.UseRestoDbLoggingMiddleware(builder.Configuration);

await app.MapRestoDbRoutesAsync(builder.Configuration);

app.UseRestoDbCors(builder.Configuration);
app.UseRestoDbAuthentication(builder.Configuration);
app.UseRestoDbSwaggerUi(builder.Configuration);
app.UseRestoDBRateLimiter(builder.Configuration);

await app.RunAsync();
