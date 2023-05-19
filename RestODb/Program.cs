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
builder.Services.UseRestoDBDbProvider(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddRestoDbSwagger(builder.Configuration);

builder.Services.AddRestoDbJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

app.UseRestoDbLoggingMiddleware(builder.Configuration);

await app.MapRestoDbRoutesAsync(builder.Configuration);


app.UseRestoDbAuthentication(builder.Configuration);
app.UseRestoDbSwaggerUi(builder.Configuration);

await app.RunAsync();
