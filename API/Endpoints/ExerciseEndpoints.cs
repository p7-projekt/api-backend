using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Contracts.Repositories;
using Core.Contracts.Services;
using Core.Exercises.Models;
using FluentResults;
using Core.Shared;
using Infrastructure.Authentication.Models;
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

        exerciseV1.MapPost("/", async Task<Results<Created, Ok<Result>>>([FromBody]ExerciseDto dto, ISolutionRunnerService solutionRunner, ClaimsPrincipal principal, IExerciseRepository exerciseRepo) =>
        {
            var result = await solutionRunner.SubmitSolutionAsync(new ExerciseSubmissionDto(dto.Solution, dto.InputParameterType, dto.OutputParamaterType, dto.Testcases));

            if (result.IsFailed)
            {

                return TypedResults.Ok(result);
            }

            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            await exerciseRepo.InsertExerciseAsync(dto, Convert.ToInt32(userId));
            return TypedResults.Created();

        }).RequireAuthorization(nameof(Roles.Instructor)).WithRequestValidation<ExerciseDto>();
        
        
        return app;
    }
}
