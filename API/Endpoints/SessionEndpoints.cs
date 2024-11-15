using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Sessions.Contracts;
using Core.Sessions.Models; 
using Core.Shared;
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

        sessionV1Group.MapDelete("/{sessionId:int}",
            async Task<Results<NoContent, NotFound, BadRequest>> (ClaimsPrincipal principal, int sessionId,
                ISessionService service) =>
            {
                var userId = principal.FindFirst( ClaimTypes.UserData)?.Value;
                if (userId == null)
                {
                    return TypedResults.BadRequest();
                }
                
                var results = await service.DeleteSession(sessionId, Convert.ToInt32(userId));
                if (results.IsFailed)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
                
            }).RequireAuthorization(nameof(Roles.Instructor));
        
        // Get author sessions
        sessionV1Group.MapGet("/",
            async Task<Results<Ok<List<GetSessionsResponseDto>>, NotFound, BadRequest>> (ClaimsPrincipal principal,
                ISessionService sessionService) =>
            {
                var userId = principal.FindFirst( ClaimTypes.UserData)?.Value;
                if (userId == null)
                {
                    return TypedResults.BadRequest();
                }

                var results = await sessionService.GetSessions(Convert.ToInt32(userId));
                if (results.IsFailed)
                {
                    return TypedResults.NotFound();
                }
            return TypedResults.Ok(results.Value);
            }).RequireAuthorization(nameof(Roles.Instructor));
        
        // Get session
        sessionV1Group.MapGet("/{id:int}", async Task<Results<Ok<GetSessionResponseDto>, NotFound, BadRequest>>(int id, ClaimsPrincipal principal, ISessionService service) =>
        {

            var userId = principal.FindFirst( ClaimTypes.UserData)?.Value;
            var userRoles = principal.FindFirst(x => x.Type == ClaimTypes.Role)?.Value;
            if (userRoles == null)
            {
                return TypedResults.BadRequest();
            }
            
            var result = await service.GetSessionByIdAsync(id, Convert.ToInt32(userId), RolesConvert.Convert(userRoles));
            if (result.IsFailed)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(result.Value);
            
        }).RequireAuthorization(Policies.AllowAllRoles);
        
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
        app.MapPost("/join", async Task<Results<Ok<JoinSessionResponseDto>, BadRequest<ValidationProblemDetails>>> ([FromBody]JoinSessionDto dto, ISessionService service) =>
        {
            var result = await service.JoinSessionAnonUser(dto);
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