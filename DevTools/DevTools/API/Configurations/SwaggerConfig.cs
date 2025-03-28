using Microsoft.OpenApi.Models;

namespace DevTools.API.Configuration;

public static class SwaggerConfig
{
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DevTools API",
                Version = "v1",
                Description = "API for DevTools platform"
            });


            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Please enter your token with this format: ''Bearer YOUR_TOKEN''",
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });
    }

    public static void UseSwaggerUIConfig(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevTools API v1");
            c.RoutePrefix = "swagger";
        });
    }
}