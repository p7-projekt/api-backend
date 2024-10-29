using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Exercises.Models;
using Core.Shared;
using FluentResults;
using Core.Solutions.Contracts;
using Core.Solutions.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Core.Exercises.Contracts;
using Core.Exercises;

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

        exerciseV1.MapPost("/", async Task<Results<Created, BadRequest<Result>>>([FromBody]ExerciseDto dto, ISolutionRunnerService solutionRunner, ClaimsPrincipal principal, IExerciseRepository exerciseRepo) =>
        {
            var result = await solutionRunner.CreateSolutionAsync(new ExerciseSubmissionDto(dto.Solution, dto.InputParameterType, dto.OutputParamaterType, dto.Testcases));

            if (result.IsFailed)
            {

                return TypedResults.BadRequest(result);
            }

            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            await exerciseRepo.InsertExerciseAsync(dto, Convert.ToInt32(userId));
            return TypedResults.Created();

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

        exerciseV1.MapGet("/{exerciseId:int}", async Task<Results<Ok<GetExerciseResponseDto>, BadRequest>> (int exerciseId, IExerciseService exerciseService) =>
        {
            var result = await exerciseService.GetExerciseById(exerciseId);
            if (result.IsFailed)
            {
                return TypedResults.BadRequest();
            }
            return TypedResults.Ok(result.Value);
        });

        exerciseV1.MapPut("/{exerciseId:int}", async Task<Results<Ok, BadRequest<string>>> ([FromBody] ExerciseDto dto, ClaimsPrincipal principal, int exerciseId, IExerciseService exerciseService) =>
        {
            var userId = principal.FindFirst(ClaimTypes.UserData)?.Value;

            var result = await exerciseService.UpdateExercise(exerciseId, Convert.ToInt32(userId), dto);
            if (result.IsFailed)
            {
                return TypedResults.BadRequest(result.ToString());
            }
            return TypedResults.Ok();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ExerciseDto>();

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
        
        exerciseV1.MapPost("/{exerciseId:int}/submit", async Task<IResult> ([FromBody] SubmitSolutionDto dto, int exerciseId, ISolutionRunnerService service) =>
        {
            var result = await service.SubmitSolutionAsync(dto);
            if (result.IsFailed)
            {
                return TypedResults.BadRequest(result.Errors);
            }

            return TypedResults.Ok();
        }).WithRequestValidation<SubmitSolutionDto>();

        return app;
    }
}
