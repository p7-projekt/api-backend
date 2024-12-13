using System.Reflection;
using Core.Classrooms;
using Core.Classrooms.Contracts;
using Core.Dashboards;
using Core.Dashboards.Contracts;
using Core.Exercises;
using Core.Exercises.Contracts;
using Core.Languages.Contracts;
using Core.Languages.Services;
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
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IDashboardService, DashboardService>();

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
                        schedule => schedule.WithIntervalInMinutes(2).RepeatForever());
                });
        });
        
        return services;
    }
}