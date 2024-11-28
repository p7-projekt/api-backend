using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Core.Classrooms.Models;
using Core.Exercises.Models;
using Core.Shared;
using System.Security.Claims;
using Core.Classrooms.Contracts;
using Microsoft.AspNetCore.Mvc;
using API.Configuration;
using FluentValidation.Internal;
using Infrastructure.Authentication.Models;
using FluentResults;
using API.Endpoints.Shared;


namespace API.Endpoints;

public static class ClassroomEndpoints
{
    public static WebApplication UseClassroomEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
    {
        var classroomV2 = app.MapGroup("v{version:apiVersion}/classrooms").WithApiVersionSet(apiVersionSet).MapToApiVersion(2).WithTags("Classroom").WithOpenApi();

        classroomV2.MapPost("/", async Task<Results<Created, BadRequest>> ([FromBody] ClassroomDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.CreateClassroom(dto, Convert.ToInt32(userId));

            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Created();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ClassroomDto>();

        classroomV2.MapPost("/{classroomId:int}/session", async Task<Results<Created, BadRequest>> (int classroomId, [FromBody] ClassroomSessionDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.AddSessionToClassroom(dto, Convert.ToInt32(userId), classroomId);

            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Created();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ClassroomSessionDto>();

        classroomV2.MapGet("/{classroomId:int}", async Task<Results<Ok<GetClassroomResponseDto>, BadRequest<ValidationProblemDetails>>> (int classroomId, ClaimsPrincipal principal, IClassroomService service) =>
        {

            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var userRole = principal.Claims.First(c => c.Type == ClaimTypes.Role).Value;

            var result = await service.GetClassroomById(classroomId, int.Parse(userId), RolesConvert.Convert(userRole));
            if(result.IsFailed)
            {
                return TypedResults.BadRequest(CreateBadRequest.CreateValidationProblemDetails(result.Errors, "Errors", "Errors"));
            }

            if (userRole == Roles.Student)
            {
                result.Value.Roomcode = null;
                result.Value.IsOpen = null;
            }
            
            
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(Policies.AllowClassroomRoles);

        classroomV2.MapGet("/", async Task<Results<Ok<List<GetClassroomsResponseDto>>, BadRequest>> (ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var userRole = principal.Claims.First(c => c.Type == ClaimTypes.Role).Value;

            var result = await service.GetClassroomsByUserRole(Convert.ToInt32(userId), RolesConvert.Convert(userRole));
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(Policies.AllowClassroomRoles);

        classroomV2.MapDelete("/{classroomId:int}", async Task<Results<NoContent, BadRequest>> (int classroomId, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.DeleteClassroom(classroomId, Convert.ToInt32(userId));

            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();

        }).RequireAuthorization(nameof(Roles.Instructor));

        classroomV2.MapPut("/{classroomId:int}", async Task<Results<NoContent, BadRequest>> (int classroomId, [FromBody] UpdateClassroomDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

            var result = await service.UpdateClassroomDetails(dto, classroomId, Convert.ToInt32(userId));
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<UpdateClassroomDto>();

        classroomV2.MapPut("/{classroomId:int}/session", async Task<Results<NoContent, BadRequest>> (int classroomId, [FromBody] UpdateClassroomSessionDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

            var result = await service.UpdateClassroomSession(dto, classroomId, Convert.ToInt32(userId));
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<UpdateClassroomSessionDto>();

        classroomV2.MapDelete("/session/{sessionId:int}", async Task<Results<NoContent, BadRequest>> (int sessionId, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

            var result = await service.DeleteClassroomSession(sessionId, Convert.ToInt32(userId));
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();

        }).RequireAuthorization(nameof(Roles.Instructor));

        classroomV2.MapGet("/session/{sessionId:int}", async Task<Ok<GetClassroomSessionResponseDto>> (int sessionId, IClassroomService service) =>
        {
            var result = await service.GetClassroomSessionById(sessionId);

            return TypedResults.Ok(result);

        }).RequireAuthorization(Policies.AllowClassroomRoles);

        classroomV2.MapDelete("/{classroomId:int}/leave", async Task<Results<NoContent, BadRequest>> (int classroomId, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            
            var result = await service.LeaveClassroom(classroomId, Convert.ToInt32(userId));
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();
        }).RequireAuthorization(nameof(Roles.Student));

        return app;
    }
}
