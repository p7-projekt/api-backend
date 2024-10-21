using System.Reflection;
using Core.Contracts.Services;
using Core.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class RegisterCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(new [] {Assembly.GetExecutingAssembly() });
        services.AddScoped<StudentService>();
        services.AddScoped<ISolutionRunnerService, SolutionRunnnerService>();

        return services;
    }
}