using Core;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class RegisterInfrastructureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }
}