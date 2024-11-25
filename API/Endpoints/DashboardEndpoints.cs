using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Dashboards.Contracts;
using Core.Dashboards.Models;
using Core.Shared;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication UseDashboardEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var dashboardV1Group = app.MapGroup("v{version:apiVersion}/dashboard").WithApiVersionSet(apiVersionSet)
            .WithTags("Dashboard");

        //Get exercises in timed_session
        dashboardV1Group.MapGet("/timedSession/{sessionId:int}", async Task<Results<Ok<GetExercisesInSessionCombinedInfo>, NotFound, BadRequest>> (int sessionId, ClaimsPrincipal principal,
                IDashboardService dashboardService) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await dashboardService.GetExercisesInTimedSession(sessionId, int.Parse(userId));

            if (result.IsFailed)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(nameof(Roles.Instructor));

        //Get exercises in timed_session
        dashboardV1Group.MapGet("/classSession/{sessionId:int}", async Task<Results<Ok<GetExercisesInSessionCombinedInfo>, NotFound, BadRequest>> (int sessionId, ClaimsPrincipal principal,
                IDashboardService dashboardService) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await dashboardService.GetExercisesInTimedSession(sessionId, int.Parse(userId));

            if (result.IsFailed)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(nameof(Roles.Instructor));

        dashboardV1Group.MapGet("/solution/{exerciseId}", async Task<Results<Ok<GetExerciseSolutionResponseDto>, NotFound, BadRequest>> (int exerciseId, ClaimsPrincipal principal,
            IDashboardService dashboardService) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            var result = await dashboardService.GetExerciseSolution(exerciseId, int.Parse(userId));
            if (result.IsFailed)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization (nameof(Roles.Instructor));

        return app;
    }
}
