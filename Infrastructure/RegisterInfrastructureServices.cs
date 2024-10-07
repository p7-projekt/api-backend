using Core;
using Core.Shared.Contracts;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class RegisterInfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Check which environment is configured in order to create the correct connection
        string connectionString = "";
        if (IsDevelopment())
        {
            connectionString = configuration.GetConnectionString("Postgres")!;
        }
        else
        {
            connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING")!;
        }
        // Register the Npgsqlconnectionfactory
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }

    public static bool IsDevelopment()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}