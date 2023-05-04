using System.Diagnostics;

namespace RestODb.Core
{
    public static class AppBuilderLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRestoDbLoggingMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        {
            string apiSegment = configuration.GetValue("ApiSegment", "api")!;

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

            return app;
        }
    }
}
