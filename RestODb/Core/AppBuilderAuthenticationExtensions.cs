using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RestODb.Core
{
    public static class AppBuilderAuthenticationExtensions
    {
        public static IServiceCollection AddRestoDbJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            bool authEnabled = configuration.GetValue<bool>("Auth:Enabled");

            if (authEnabled)
                services
                .AddAuthorization()
                .AddAuthentication("MyBearer")
                .AddJwtBearer("MyBearer", x =>
                {
                    AuthProviderOptions? authSection =
                        configuration.GetSection("Authentication").Get<AuthProviderOptions>() ?? throw new Exception("Auth section must exists");

                    x.RequireHttpsMetadata = authSection.RequireHttpsMetadata;
                    x.SaveToken = authSection.SaveToken;
                    x.Authority = authSection.Issuer;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = authSection.ValidateLifetime,

                        //issuer
                        ValidIssuer = authSection.Issuer,
                        ValidateIssuer = authSection.ValidateIssuer,

                        //audience
                        ValidAudience = authSection.Audience,
                        ValidateAudience = authSection.ValidateAudience,

                        NameClaimType = authSection.NameClaimType,
                        RoleClaimType = authSection.RoleClaimType,
                    };

                    if (authSection.ValidateIssuerSigningKey)
                    {
                        x.TokenValidationParameters.ValidateIssuerSigningKey = authSection.ValidateIssuerSigningKey;
                        x.TokenValidationParameters.ValidAlgorithms = authSection.ValidAlgorithms;

                        x.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSection.Key));
                    }
                });

            return services;
        }

        public static IApplicationBuilder UseRestoDbAuthentication(this IApplicationBuilder app, IConfiguration configuration)
        {
            bool authEnabled = configuration.GetValue<bool>("Auth:Enabled");
            if (authEnabled)
            {
                app.UseAuthentication().UseAuthorization();
            }

            return app;
        }

    }

    public class AuthProviderOptions
    {
        public bool Enabled { get; set; } = false;
        public bool RequireHttpsMetadata { get; set; } = false;
        public bool SaveToken { get; set; } = true;
        public bool ValidateIssuerSigningKey { get; set; } = true;
        public bool ValidateAudience { get; set; } = false;
        public bool ValidateIssuer { get; set; } = false;
        public bool ValidateLifetime { get; set; } = false;
        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int TokenLifeTime { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        public string NameClaimType { get; set; } = "prefered_username";
        public string RoleClaimType { get; set; } = "role";
        public IEnumerable<string> ValidAlgorithms { get; set; } = new[] { "HS256" };
    }
}
