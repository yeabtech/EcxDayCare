using Microsoft.OpenApi;

namespace ECX_Daycare.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger/OpenAPI configuration to the service collection.
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ECX Daycare API",
                Version = "v1",
                Description = "Daycare Management System API with Role-Based Access Control"
            });

            // Define the JWT Bearer security scheme
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token. Example: Bearer {your_token}"
            });

            // Require JWT authentication globally (Swashbuckle 10 changed to Func overload)
            options.AddSecurityRequirement(_ =>
            {
                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(
                    new OpenApiSecuritySchemeReference("Bearer", null),
                    new List<string>());
                return requirement;
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the Swagger UI middleware in the HTTP pipeline.
    /// </summary>
    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Daycare API v1");
                options.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}
