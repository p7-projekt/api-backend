using System.Reflection;
using Core.Sessions;
using Core.Sessions.Contracts;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class RegisterCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(new [] {Assembly.GetExecutingAssembly() });
        services.AddScoped<StudentService>();
        services.AddScoped<ISessionService, SessionService>();


        return services;
    }
}