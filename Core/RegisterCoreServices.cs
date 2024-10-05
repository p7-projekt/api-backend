using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class RegisterCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {

        services.AddScoped<StudentService>();


        return services;
    }
}