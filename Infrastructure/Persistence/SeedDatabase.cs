using System.Reflection;
using Core.Shared;
using Dapper;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Infrastructure.Persistence.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class SeedDatabase
{
    public static IServiceProvider SeedAdminAccount(this IServiceProvider serviceProvider)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("SEED_ADMIN"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider;
        }
        using var scope = serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("AdminAccountSeed");
        var passwordHasher = new PasswordHasher<User>();
        var user = new User
        {
            CreatedAt = DateTime.UtcNow,
            Name = "Admin",
            Email = Environment.GetEnvironmentVariable("ADMIN_MAIL") ??
                    throw new NullReferenceException("ADMIN_MAIL")
        };
        var hashedPassword = passwordHasher.HashPassword(user, 
            Environment.GetEnvironmentVariable("ADMIN_PASS") ?? throw new NullReferenceException("ADMIN_PASS"));
        user.PasswordHash = hashedPassword;
        var result = userRepo!.IsEmailAvailableAsync(user.Email).WaitAsync(CancellationToken.None).Result;
        if (!result)
        {
            return serviceProvider;
        }
        var createUser = userRepo.CreateUserAsync(user, Roles.Instructor).Result;
        if (createUser.IsFailed)
        {
            logger!.LogWarning($"Failed to create user {user.Email}"); 
            return serviceProvider;
        }
        logger!.LogInformation($"User {user.Email} has been created");
        return serviceProvider;
    }
    
    public static IServiceProvider DevelopmentSeed(this IServiceProvider serviceProvider)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                StringComparison.OrdinalIgnoreCase))
        {
            return serviceProvider;
        }
        using var scope = serviceProvider.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("DevelopmentSeed");
        var dbFactory = serviceProvider.GetService<IDbConnectionFactory>();
        if (IsDatabaseSeeded(dbFactory!))
        {
            logger!.LogInformation("Database does already contain data.");
            return serviceProvider;
        }
        logger!.LogInformation("Database is not seeded seeding...");
        var seedQueries = GetSeedQueries(logger!);
        using var con = dbFactory!.CreateConnectionAsync().WaitAsync(CancellationToken.None).Result;
        con.Query(seedQueries);
        logger!.LogInformation("Database is seeded!");
        return serviceProvider;
    }

    private static bool IsDatabaseSeeded(IDbConnectionFactory factory)
    {
        using var con = factory.CreateConnectionAsync().WaitAsync(CancellationToken.None).Result;
        var query = "SELECT COUNT(*) FROM exercise;";
        var result = con.Query<int>(query);
        if (result.First() > 0)
        {
            return true;
        }

        return false;
    }
    private static string GetSeedQueries(ILogger logger)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var dummydata = assembly.GetManifestResourceNames().Single(str => str.EndsWith("DummyData.sql"));
        using var stream = assembly.GetManifestResourceStream(dummydata);
        if (stream == null)
        {
            logger.LogWarning("Could not find DummyData.sql");
            return string.Empty;
        }
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}