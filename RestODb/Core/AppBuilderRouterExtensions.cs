using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SqlKata;
using SqlKata.Execution;
using System.Diagnostics;
using System.Text;

namespace RestODb.Core
{
    public static class AppBuilderRouterExtensions
    {
        public static async Task<IApplicationBuilder> MapRestoDbRoutesAsync(this WebApplication app, IConfiguration configuration)
        {
            bool authEnabled = configuration.GetValue<bool>("Auth:Enabled");
            bool rateLimiterEnabled = configuration.GetValue<bool>("RateLimiter:Enabled");

            string apiSegment = configuration.GetValue("ApiSegment", "api")!;
            var logger = app.Services.GetService<ILogger<Program>>()!;
            Stopwatch sw = Stopwatch.StartNew();

            //get available routes
            var sqlKata = app.Services.GetService<SqlKataQueryFactory>() ?? throw new Exception("Cannot find service of type SqlKataQueryFactory");
            var tables = (await sqlKata.GetTablesListAsync()).ToList();
            logger.LogDebug("Found {tableCount} exposable entities in database", tables.Count);

            //limit to configured entities
            var limitTo = configuration.GetSection("limitTo").Get<string[]>();
            if (limitTo?.Any() == true)
            {
                tables = tables.FindAll(t => limitTo.Contains(t));
                logger.LogDebug("Limiting to entities {@entities}", limitTo);
            }

            //cache
            bool cacheEnabled = configuration.GetValue("Cache:Enabled", false);
            int cacheDuration = configuration.GetValue("Cache:DurationInSeconds", 60);
            bool isCacheAbsolute = configuration.GetValue("Cache:Absolute", true);
            logger.LogDebug("Cache enabled: {cacheEnabled}, duration: {cacheDuration}, absolute: {isCacheAbsolute}", cacheEnabled, cacheDuration, isCacheAbsolute);

            //csv
            bool csvEnabled = configuration.GetValue("Csv:Enabled", true);
            string csvSeparator = configuration.GetValue("Csv:Separator", ",") ?? ",";
            logger.LogDebug("Csv enabled: {csvEnabled}, separator: {csvSeparator}", csvEnabled, csvSeparator);

            //create routes (create individual routes so they can appear in swagger
            foreach (var table in tables)
            {
                var description = await sqlKata.GetEntityColumnsDescription(table);
                string routePath = $"/{apiSegment}/{table}";
                var route = app.MapGet(routePath, HandleAsync)
                               .Produces(200, responseType: TypeBuilderHelper.BuildTypeForEntity(table, description), contentType: "application/json")
                               .WithName(table)
                               .WithOpenApi();

                string csvRoutePath = $"{routePath}/csv";
                var csvRoute = csvEnabled ? app.MapGet(csvRoutePath, async (HttpContext httpContext, [FromServices] SqlKataQueryFactory factory, [FromServices] IMemoryCache cache, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc) =>
                {
                    httpContext.Response.ContentType = "text/csv";

                    var results = await HandleAsync(factory, cache, select, skip, take, orderBy, orderByDesc);

                    if (results?.Any() == true)
                    {
                        StringBuilder sb = new();
                        var keys = (results.First() as IDictionary<string, object>).Keys;

                        string headers = string.Join(csvSeparator, keys.Select(k => $"\"{k}\"")) + "\n";
                        await httpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(headers));

                        foreach (var result in results)
                        {
                            var value = (result as IDictionary<string, object>).Values;
                            string line = string.Join(csvSeparator, value.Select(v => $"\"{v}\"")) + "\n";
                            await httpContext.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(line));
                            await httpContext.Response.BodyWriter.FlushAsync();
                        }

                        await httpContext.Response.BodyWriter.CompleteAsync();
                        return Results.Ok();
                    }

                    return Results.NoContent();
                })
                    .Produces(204)
                    .Produces(200, contentType: "text/csv").WithName($"{table}-csv").WithOpenApi()
                    : null;

                Task<IEnumerable<dynamic>?> HandleAsync([FromServices] SqlKataQueryFactory factory, [FromServices] IMemoryCache cache, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc)
                {
                    if (!cacheEnabled) return ExecuteAsync(factory, table, select, skip, take, orderBy, orderByDesc);

                    else return cache.GetOrCreateAsync($"{table}::{select}_{skip}_{take}_{orderBy}_{orderByDesc}", item =>
                    {
                        logger.LogDebug("(Re)creating cache for key {cacheKey}", item.Key);

                        if (isCacheAbsolute) item.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDuration));
                        else item.SetSlidingExpiration(TimeSpan.FromSeconds(cacheDuration));

                        return ExecuteAsync(factory, table, select, skip, take, orderBy, orderByDesc);
                    });
                }

                if (authEnabled)
                {
                    route.RequireAuthorization();
                    if (csvEnabled) csvRoute.RequireAuthorization();
                }
                if (rateLimiterEnabled)
                {
                    route.RequireRateLimiting(AppBuilderRateLimiterExtensions.PolicyName);
                    if (csvEnabled) csvRoute.RequireRateLimiting(AppBuilderRateLimiterExtensions.PolicyName);
                }
                logger.LogInformation("Route {routePath} (GET) mapped", routePath);
                if (csvEnabled) logger.LogInformation("Route {csvRoutePath} (GET) mapped", csvRoutePath);
            }
            logger.LogInformation("Mapped {tableCount} routes in {elapsed}ms", tables.Count, sw.ElapsedMilliseconds);

            return app;

            Task<IEnumerable<dynamic>?> ExecuteAsync(SqlKataQueryFactory factory, string table, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc)
            {
                Query sqlQuery = factory.Create(table);

                if (!string.IsNullOrWhiteSpace(select)) sqlQuery = sqlQuery.Select(select.SafeSplit());

                if (take.HasValue) sqlQuery = sqlQuery.Take(take.Value);

                if (skip.HasValue) sqlQuery = sqlQuery.Skip(skip.Value);

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    string[] parts = orderBy.SafeSplit();
                    sqlQuery = orderByDesc.HasValue && orderByDesc.Value ? sqlQuery.OrderByDesc(parts) : sqlQuery.OrderBy(parts);
                }

                return sqlQuery.GetAsync();
            }
        }
    }
}
