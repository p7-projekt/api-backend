using System.Net;
using System.Text;
using Asp.Versioning;
using Core.Shared;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace API.Configuration;

public static class RegisterApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        // Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        // Global exception handling
        services.AddExceptionHandler<GlobalExceptionHandler>();


        // CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        policy.WithOrigins("localhost:5173")
                            .WithMethods(HttpMethods.Get, HttpMethods.Patch, HttpMethods.Delete, HttpMethods.Post, HttpMethods.Put)
                            .AllowAnyHeader();
                    }
                }
                );
        });

        services.AddRolePolicies(); 
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthConstants.Issuer,
                ValidAudience = AuthConstants.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(AuthConstants.JwtSecret)!))
            };
        });
        
        return services;
    }

    private static IServiceCollection AddRolePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(opt =>
        {
            opt.AddPolicy(nameof(Roles.Instructor), policy => policy.RequireRole(nameof(Roles.Instructor)));
            opt.AddPolicy(nameof(Roles.Student), policy => policy.RequireRole(nameof(Roles.Student)));
            opt.AddPolicy(nameof(Roles.AnonymousUser), policy => policy.RequireRole(nameof(Roles.AnonymousUser)));
            opt.AddPolicy(Policies.AllowAllRoles, policy => policy.RequireRole(nameof(Roles.Instructor), nameof(Roles.Student), nameof(Roles.AnonymousUser)));
            opt.AddPolicy(Policies.AllowSubmissions, policy => policy.RequireRole(nameof(Roles.Student), nameof(Roles.AnonymousUser)));
            opt.AddPolicy(Policies.AllowClassroomRoles, policy => policy.RequireRole(nameof(Roles.Student), nameof(Roles.Instructor)));
        });

        return services;
    }
}

public static class Policies
{
    public const string AllowAllRoles = "AllowAllRoles";
    public const string AllowSubmissions = "AllowSubmissions";
    public const string AllowClassroomRoles = "AllowClassroomRoles";
}