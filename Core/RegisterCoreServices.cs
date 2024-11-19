using System.Reflection;
using Core.Exercises;
using Core.Exercises.Contracts;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Solutions;
using Core.Solutions.Contracts;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Core;

public static class RegisterCoreServices
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(new [] {Assembly.GetExecutingAssembly() });
        services.AddScoped<ISolutionRunnerService, SolutionRunnerService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IExerciseService, ExerciseService>();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        services.AddQuartz(configure =>
        {
            var job = new JobKey(nameof(SessionExpirationJob));
            configure.AddJob<SessionExpirationJob>(job)
                .AddTrigger(trigger =>
                {
                    trigger.ForJob(job).WithSimpleSchedule(
                        schedule => schedule.WithIntervalInMinutes(5).RepeatForever());
                });
        });
        
        return services;
    }
}