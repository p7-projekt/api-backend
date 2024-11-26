using System.Reflection;
using Core;
using Core.Classrooms.Contracts;
using Core.Dashboards.Contracts;
using Core.Exercises.Contracts;
using Core.Languages.Contracts;
using Core.Sessions.Contracts;
using Core.Shared.Contracts;
using Core.Solutions.Contracts;
using DbUp;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure;

public static class RegisterInfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Register the Npgsqlconnectionfactory
        string connectionString = GetConnectionString(configuration);
        services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(connectionString));
        // Migrate schema
        EnsureMigration(connectionString);
        
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        
        // Authentication - Authorization
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAnonTokenService, TokenService>();
        services.AddValidatorsFromAssemblies(new [] {Assembly.GetExecutingAssembly() });
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ISolutionRepository, SolutionRepository>();
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        return services;
    }


    private static void EnsureMigration(string connectionString)
    {
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => !s.EndsWith("DummyData.sql"))
            .LogToConsole()
            .Build();
        
        var result = upgrader.PerformUpgrade();
        
        if (!result.Successful)
        {
            Console.WriteLine(result.Error);
        }
    }
    
    private static string GetConnectionString(ConfigurationManager configuration)
    {
        if (IsDevelopment())
        {
            return configuration.GetConnectionString("Postgres")!;
        }

        return Environment.GetEnvironmentVariable("CONNECTIONSTRING")!;
    }

    private static bool IsDevelopment()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}