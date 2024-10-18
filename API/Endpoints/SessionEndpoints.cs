using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using FluentResults;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class SessionEndpoints
{
    public static WebApplication UseSessionEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var sessionV1Group = app.MapGroup("v{version:apiVersion}/sessions").WithApiVersionSet(apiVersionSet)
            .WithTags("Sessions");
        
        // Get session
        sessionV1Group.MapGet("/{id:int}", () =>
        {
            return "get session";
        });
        
        // Create session
        sessionV1Group.MapPost("/", async Task<Results<Created<CreateSessionResponseDto>, BadRequest<ValidationProblemDetails>>> ([FromBody] CreateSessionDto dto, ClaimsPrincipal principal, ISessionService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.CreateSessionAsync(dto, int.Parse(userId));
            if (result.IsFailed)
            {
                var error = CreateBadRequest.CreateValidationProblemDetails(result.Errors,
                    "Error creating session");
                return TypedResults.BadRequest(error);
            }

            return TypedResults.Created("", result.Value);

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<CreateUserDto>();
        
        // Join session
        sessionV1Group.MapPost("/{id:int}/students", () =>
        {
            
        });
        return app;
    }
}