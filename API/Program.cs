using Core;
using Infrastructure;
using Infrastructure.Persistence;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddConsole();
        
        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddCoreServices();
        
        
        // API Configuration
        builder.Services.AddApiConfiguration();
        builder.Services.AddInfrastructure(builder.Configuration);
        
        
        // API Configuration
        builder.Services.AddApiConfiguration();
        builder.Services.AddProblemDetails();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        
        app.UseExceptionHandler();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.Services.DevelopmentSeed();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseStudentEndpoints();
        app.UseCors();

        app.Run();
    }
}