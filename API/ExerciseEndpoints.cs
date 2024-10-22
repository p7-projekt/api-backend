using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Core.Example;
using Core.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using API.Configuration;
using Infrastructure.Authentication.Models;
using Core.Contracts.Services;
using System.Security.Claims;
using Core.Contracts.Repositories;

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

        exerciseV1.MapPost("/", async ([FromBody]ExerciseDto dto, ISolutionRunnerService solutionRunner, ClaimsPrincipal principal, IExerciseRepository exerciseRepo) =>
        {
            await solutionRunner.SubmitSolutionAsync(dto);

            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            await exerciseRepo.InsertExerciseAsync(dto, Convert.ToInt32(userId));

            TypedResults.Ok(dto);
        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ExerciseDto>();
        
        
        return app;
    }
}
