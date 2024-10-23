using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Exercises.Models;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

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
