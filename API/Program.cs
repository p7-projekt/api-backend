using API.Configuration;
using API.Endpoints;
using Core;
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
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.Services.DevelopmentSeed();
        }
        
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.UseSerilogRequestLogging();

        // Endpoints
        app.UseStudentEndpoints();
        app.UseExampleEndpoints();
        app.UseAuthenticationEndpoints();
        app.UseSessionEndpoints();

        app.Run();
    }
}