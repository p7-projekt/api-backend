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

namespace API.Endpoints;

public static class ClassroomEndpoints
{
    public static WebApplication UseClassroomEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();

        var classroomV2 = app.MapGroup("v{version:apiVersion}/classrooms").WithApiVersionSet(apiVersionSet).WithTags("Classroom").WithOpenApi();

        classroomV2.MapPost("/", async Task<Results<Created, BadRequest>>([FromBody]ClassroomDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.CreateClassroom(dto, Convert.ToInt32(userId));

            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Created();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ClassroomDto>();

        classroomV2.MapPost("/{classroomId:int}/session", async Task<Results<Created, BadRequest>>(int classroomId, [FromBody]ClassroomSessionDto dto, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.AddSessionToClassroom(dto, Convert.ToInt32(userId), classroomId);
            
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Created();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ClassroomSessionDto>();

        classroomV2.MapGet("/{classroomId:int}", async Task<Ok<GetClassroomResponseDto>> (int classroomId, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.GetClassroomById(classroomId);

            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(Policies.AllowClassroomRoles);

        classroomV2.MapGet("", async Task<Results<Ok<List<GetClassroomsResponseDto>>, BadRequest>> (ClaimsPrincipal principal, IClassroomService service) =>
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

        classroomV2.MapDelete("{classroomId:int}", async Task<Results<NoContent, BadRequest>> (int classroomId, ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await service.DeleteClassroom(classroomId, Convert.ToInt32(userId));

            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.NoContent();
        }).RequireAuthorization(nameof(Roles.Instructor));

        classroomV2.MapPut("{classroomId:int}", async Task<Results<NoContent, BadRequest>> (int classroomId, [FromBody]UpdateClassroomDto dto ,ClaimsPrincipal principal, IClassroomService service) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;

            var result = await service.UpdateClassroomDetails(dto, classroomId, Convert.ToInt32(userId));

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<UpdateClassroomDto>();

        return app;
    }
}
