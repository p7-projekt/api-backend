using API.Configuration;
using API.Endpoints;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Core.Solutions.Services;
using Infrastructure;
using Infrastructure.Persistence;
using Serilog;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
        
        // Supported languages
        builder.Services.AddHttpClient<IMozartService, MozartService>()
            .SetHandlerLifetime(TimeSpan.FromSeconds(30)); 
        // builder.Services.AddHttpClient<IHaskellService, HaskellService>()
            // .SetHandlerLifetime(TimeSpan.FromSeconds(30));
        
        // Add services to the container.
        builder.Services.AddCoreServices();
        builder.Services.AddInfrastructure(builder.Configuration);
        
        
        // API Configuration
        builder.Services.AddApiConfiguration();
        builder.Services.AddProblemDetails();
        

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        
        var app = builder.Build();

        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            // app.Services.DevelopmentSeed();
        }
        // Seed admin account
        app.Services.SeedAdminAccount();
        
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.UseSerilogRequestLogging();

        // Endpoints
        app.UseExerciseEndpoints(apiVersionSet);
        app.UseAuthenticationEndpoints();
        app.UseSessionEndpoints();
        app.UseUserEndpoints();
        app.UseClassroomEndpoints();
        app.UseLanguageEndpoints();

        app.Run();
    }
}