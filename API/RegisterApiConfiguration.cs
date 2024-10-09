using API.Configuration;
using Asp.Versioning;

namespace API;

public static class RegisterApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        // Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
      
        // Global exception handling
        services.AddExceptionHandler<GlobalExceptionHandler>();


        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins("localhost:5173")
                            .WithMethods(HttpMethods.Get, HttpMethods.Patch, HttpMethods.Delete, HttpMethods.Post)
                            .AllowAnyHeader();
                    }
                }
                );
        });
        
        return services;
    }
}