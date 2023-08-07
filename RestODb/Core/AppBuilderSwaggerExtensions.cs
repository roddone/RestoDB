using Microsoft.OpenApi.Models;

namespace RestODb.Core;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddRestoDbSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        bool swaggerEnabled = configuration.GetValue<bool>("EnableSwagger");
        if (swaggerEnabled) services.AddEndpointsApiExplorer().AddSwaggerGen(
            options =>
            {
                options.AddSecurityDefinition("Bearer",
                                new OpenApiSecurityScheme { Description = "JWT Authorization", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                            {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header
                            },
                            new List<string>()
                        }
                            });
            });

        return services;
    }
    public static IApplicationBuilder UseRestoDbSwaggerUi(this IApplicationBuilder application, IConfiguration configuration)
    {
        bool swaggerEnabled = configuration.GetValue<bool>("EnableSwagger");
        if (swaggerEnabled)
            application.UseSwagger().UseSwaggerUI();

        return application;
    }
}
