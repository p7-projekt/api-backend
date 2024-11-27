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
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();

        var dashboardV2Group = app.MapGroup("v{version:apiVersion}/dashboard").WithApiVersionSet(apiVersionSet)
            .WithTags("Dashboard").WithOpenApi();

        dashboardV2Group.MapGet("/{sessionId:int}", async Task<Results<Ok<GetExercisesInSessionCombinedInfo>, NotFound, BadRequest>> (int sessionId, ClaimsPrincipal principal,
        IDashboardService dashboardService) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            if (userId == null)
            {
                return TypedResults.BadRequest();
            }

            var result = await dashboardService.GetExercisesInSession(sessionId, int.Parse(userId));

            if (result.IsFailed)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization(nameof(Roles.Instructor));

        dashboardV2Group.MapGet("/solution/{exerciseId}/{appUserId}", async Task<Results<Ok<GetExerciseSolutionResponseDto>, NotFound, ForbidHttpResult, BadRequest>> (int exerciseId, int appUserId, ClaimsPrincipal principal,
            IDashboardService dashboardService) =>
        {
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
            if (userId == null)
            {
                return TypedResults.BadRequest();
            }

            var result = await dashboardService.GetUserSolution(exerciseId, appUserId, int.Parse(userId));
            if (result.IsFailed)
            {
                var errorReason = result.Errors.FirstOrDefault()?.Message;
                if (errorReason == "Not autherized")
                {
                    return TypedResults.Forbid();
                }
                else if (errorReason == "Not found")
                {
                    return TypedResults.NotFound();
                }
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(result.Value);

        }).RequireAuthorization (nameof(Roles.Instructor));

        return app;
    }
}
