using System.Reflection;
using Dapper;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class SeedDatabase
{
    public static IServiceProvider UseDummyData(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = serviceProvider.GetService<ILogger>();
        var dbFactory = serviceProvider.GetService<IDbConnectionFactory>();
        var seedQueries = GetSeedQueries(logger!);

        using var con = dbFactory!.CreateConnectionAsync().WaitAsync(CancellationToken.None).Result;

        con.Query(seedQueries);
        
        return serviceProvider;
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