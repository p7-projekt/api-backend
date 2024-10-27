using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Exercises.Models;
using Core.Sessions;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
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

        exerciseV1.MapGet("/", async Task<Results<Ok<List<GetExercisesResponseDto>>, BadRequest, NotFound>> (ClaimsPrincipal principal, IExerciseService exerciseService) => 
            {
                var userId = principal.FindFirst(ClaimTypes.UserData)?.Value;
                if (userId == null)
                {
                    return TypedResults.BadRequest();
                }

                var results = await exerciseService.GetExercises(Convert.ToInt32(userId));
                if (results.IsFailed)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.Ok(results.Value);
            }).RequireAuthorization(nameof(Roles.Instructor));

        exerciseV1.MapDelete("/{exerciseId:int}", async Task<Results<NoContent, NotFound>> (ClaimsPrincipal principal, int exerciseId, IExerciseService exerciseService) =>
            {
                var userId = principal.FindFirst(ClaimTypes.UserData)?.Value;
                if (userId == null)
                {
                    return TypedResults.NotFound();
                }

                var results = await exerciseService.DeleteExercise(exerciseId, Convert.ToInt32(userId));
                if (results.IsFailed)
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NoContent();
            }).RequireAuthorization(nameof(Roles.Instructor));

        return app;
    }
}
