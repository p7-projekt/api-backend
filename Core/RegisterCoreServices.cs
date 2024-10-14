using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class RegisterCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(new [] {Assembly.GetExecutingAssembly() });
        services.AddScoped<StudentService>();


        return services;
    }
}