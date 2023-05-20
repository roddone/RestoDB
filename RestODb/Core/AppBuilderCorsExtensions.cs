using RestODb.Core;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
}

public static class AppBuilderCorsExtensions
{

    /// <summary>
    /// Use cors in application
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRestoDbCors(this IApplicationBuilder application, IConfiguration config)
    {
        bool enabled = config.GetValue<bool>("Cors:Enabled");

        if (enabled)
            application.UseCors(corsConfig =>
            {
                var allowedHeaders = config.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? Array.Empty<string>();

                if (allowedHeaders.IsNullOrEmpty())
                    corsConfig.AllowAnyHeader();
                else
                    corsConfig.WithHeaders(allowedHeaders);

                var allowedMethods = config.GetSection("Cors:AllowedMethods").Get<string[]>() ?? Array.Empty<string>();

                if (allowedMethods.IsNullOrEmpty())
                    corsConfig.AllowAnyMethod();
                else
                    corsConfig.WithMethods(allowedMethods);

                var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                if (allowedOrigins.IsNullOrEmpty())
                    corsConfig.AllowAnyOrigin();
                else
                    corsConfig.WithOrigins(allowedOrigins);
            });

        return application;
    }

    public static IServiceCollection AddRestoDBCors(this IServiceCollection services, IConfiguration config)
    {
        bool enabled = config.GetValue<bool>("Cors:Enabled");

        if (enabled)
            services.AddCors();

        return services;
    }
}
