using Microsoft.AspNetCore.Mvc;
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

            var limitTo = configuration.GetSection("limitTo").Get<string[]>();
            if (limitTo?.Any() == true) {
                tables = tables.FindAll(t => limitTo.Contains(t));
                logger.LogDebug("Limiting to entities {@entities}", limitTo);
            }

            //create routes (create individual routes so they can appear in swagger
            foreach (var table in tables)
            {
                string routePath = $"/{apiSegment}/{table}";
                var route = app.MapGet(routePath, ([FromServices] SqlKataQueryFactory factory, string? select, int? skip, int? take, string? orderBy, bool? orderByDesc) =>
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

                logger.LogInformation("Route {routePath} (GET) mapped", routePath);
            }
            logger.LogInformation("Mapped {tableCount} in {elapsed}ms", tables.Count, sw.ElapsedMilliseconds);

            return app;
        }

        static string[] SafeSplit(string input)
        {
            return input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
