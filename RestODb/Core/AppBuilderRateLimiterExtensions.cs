using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace RestODb.Core
{
    public static class AppBuilderRateLimiterExtensions
    {
        public const string PolicyName = "fixed";
        public static IServiceCollection AddRestoDBIpRateLimiter(this IServiceCollection services, IConfiguration config)
        {
            bool enabled = config.GetValue<bool>("RateLimiter:Enabled");

            if (enabled)
                services.AddRateLimiter(o =>
                {
                    o.RejectionStatusCode = 429;
                    o.AddFixedWindowLimiter(policyName: PolicyName, options =>
                        {
                            options.PermitLimit = config.GetValue<int>("RateLimiter:PermitLimit", 100);
                            options.Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiter:WindowInSeconds", 60));
                            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                            options.QueueLimit = config.GetValue<int>("RateLimiter:QueueLimit", 0);
                        });
                });

            return services;
        }

        public static IApplicationBuilder UseRestoDBRateLimiter(this IApplicationBuilder app, IConfiguration config)
        {
            bool enabled = config.GetValue<bool>("RateLimiter:Enabled");
            if (enabled)
                app.UseRateLimiter();

            return app;
        }
    }
}
