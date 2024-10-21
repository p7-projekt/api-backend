using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
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
        sessionV1Group.MapGet("/{id:int}", (ClaimsPrincipal principal) =>
        {
            // verify the anon user token is associated to current session:
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            // need to get the role, such that we can act accordingly as instructors can only be verified access through the users table, and for anon users in the student table.
            var role = principal.Claims.First(c => c.Type == ClaimTypes.Role).Value;
            
            
            
            return "get session";
        }).RequireAuthorization(nameof(Roles.AnonymousUser));
        
        // Create session
        sessionV1Group.MapPost("/", async Task<Results<Created<CreateSessionResponseDto>, BadRequest<ValidationProblemDetails>>> ([FromBody] CreateSessionDto dto, ClaimsPrincipal principal, ISessionService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.CreateSessionAsync(dto, int.Parse(userId));
            if (result.IsFailed)
            {
                var error = CreateBadRequest.CreateValidationProblemDetails(result.Errors,
                    "Error creating session", "Session");
                return TypedResults.BadRequest(error);
            }

            return TypedResults.Created("", result.Value);

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<CreateSessionDto>();
        
        // Join session
        sessionV1Group.MapPost("/{id:int}/participants", async Task<Results<Ok<JoinSessionResponseDto>, BadRequest<ValidationProblemDetails>>> ([FromBody] JoinSessionDto dto, int id, ISessionService service) =>
        {
            var result = await service.JoinSessionAnonUser(dto, id);
            if (result.IsFailed)
            {
                var error = CreateBadRequest.CreateValidationProblemDetails(result.Errors, $"Invalid {nameof(dto.SessionCode)}", nameof(dto.SessionCode));
                return TypedResults.BadRequest(error);
            }
            
            return TypedResults.Ok(result.Value);
        }).WithRequestValidation<JoinSessionDto>();
        return app;
    }
}