using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class SessionEndpoints
{
    public static WebApplication UseSessionEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {

        var sessionV1Group = app.MapGroup("v{version:apiVersion}/sessions").WithApiVersionSet(apiVersionSet).MapToApiVersion(1)
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
            }).RequireAuthorization(Policies.AllowClassroomRoles);
        
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
        app.MapPost("/join", async Task<Results<Ok<JoinResponseDto>, BadRequest<ValidationProblemDetails>>> ([FromBody]JoinDto dto, ISessionService service, ClaimsPrincipal principal) =>
        {
            var hasToken = principal.Claims.Any();
            
            // Join as anonUser
            if (!hasToken)
            {
                var result = await service.JoinSessionAnonUser(dto);
                if (result.IsFailed)
                {
                    var error = CreateBadRequest.CreateValidationProblemDetails(result.Errors, $"Invalid {nameof(dto.Code)}", nameof(dto.Code));
                    return TypedResults.BadRequest(error);
                }
                return TypedResults.Ok(result.Value);
            }
            
            var role = RolesConvert.Convert(principal.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            if (role != Roles.Student)
            {
                var error = CreateBadRequest.CreateValidationProblemDetails(new List<IError>{new Error("Only students can join!")}, "Error", "Errors");
                return TypedResults.BadRequest(error);
            }
            var userId = Convert.ToInt32(principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value);
            var studentResult = await service.JoinStudent(userId, dto.Code);
            if (studentResult.IsFailed)
            {
                
                var error = CreateBadRequest.CreateValidationProblemDetails(studentResult.Errors, "Error", "Errors");
                return TypedResults.BadRequest(error);
            }

            return TypedResults.Ok(studentResult.Value);
        }).WithRequestValidation<JoinDto>();

        return app;
    }
}
