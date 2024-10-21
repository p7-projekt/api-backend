using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Core.Example;
using Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using API.Configuration;
using Infrastructure.Authentication.Models;
using Core.Contracts.Services;

namespace API;

public static class ExerciseEndpoints
{
    public static WebApplication UseExerciseEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var exerciseV1 = app.MapGroup("v{version:apiVersion}/exercises").WithApiVersionSet(apiVersionSet).WithTags("Exercise");

        exerciseV1.MapPost("/", async ([FromBody]ExerciseDto dto, ISolutionRunnerService solutionRunner) =>
        {
            await solutionRunner.SubmitSolutionAsync(dto);
            TypedResults.Ok(dto);
        }).WithRequestValidation<ExerciseDto>();
        
        
        return app;
    }
}
