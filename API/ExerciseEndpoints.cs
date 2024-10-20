using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Core.Example;
using Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using API.Configuration;
using Infrastructure.Authentication.Models;

namespace API;

public static class ExerciseEndpoints
{
    public static WebApplication UseExerciseEndpouints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var exerciseV1 = app.MapGroup("v{version.apiVersion}/students").WithApiVersionSet(apiVersionSet).WithTags("Exercise");

        exerciseV1.MapPost("/", ([FromBody]ExerciseDto dto) =>
        {
            TypedResults.Ok(dto);
        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ExerciseDto>();
        
        
        return app;
    }
}
