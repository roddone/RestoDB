using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SqlKata;
using SqlKata.Execution;
using System.Diagnostics;

namespace RestODb.Core
{
    public static class AppBuilderRouterExtensions
    {
        public static async Task<IApplicationBuilder> MapRestoDbRoutesAsync(this WebApplication app, IConfiguration configuration)
        {
            bool authEnabled = configuration.GetValue<bool>("Auth:Enabled");
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

            //create routes (create individual routes so they can appear in swagger
            foreach (var table in tables)
            {
                string routePath = $"/{apiSegment}/{table}";
                var route = app.MapGet(routePath, ([FromServices] SqlKataQueryFactory factory, [FromServices] IMemoryCache cache, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc) =>
                {
                    if (!cacheEnabled) return Execute(factory, table, select, skip, take, orderBy, orderByDesc);

                    else return cache.GetOrCreateAsync($"{table}::{select}_{skip}_{take}_{orderBy}_{orderByDesc}", item =>
                        {
                            logger.LogDebug("(Re)creating cache for key {cacheKey}", item.Key);

                            if (isCacheAbsolute) item.SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDuration));
                            else item.SetSlidingExpiration(TimeSpan.FromSeconds(cacheDuration));

                            return Execute(factory, table, select, skip, take, orderBy, orderByDesc);
                        });

                }).WithName(table).WithOpenApi();

                if (authEnabled) route.RequireAuthorization();

                logger.LogInformation("Route {routePath} (GET) mapped", routePath);
            }
            logger.LogInformation("Mapped {tableCount} routes in {elapsed}ms", tables.Count, sw.ElapsedMilliseconds);

            return app;
        }

        private static Task<IEnumerable<dynamic>?> Execute(SqlKataQueryFactory factory, string table, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc)
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
