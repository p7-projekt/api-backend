using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API
{
    //public static class ExerciseEndpoints
    //{
    //    public static WebApplication UseExerciseEndpouints(this WebApplication app)
    //    {
    //        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    //            .HasApiVersion(new ApiVersion(1))
    //            .ReportApiVersions()
    //            .Build();

    //        var exerciseV1 = app.MapGroup("v{version.apiVersion}/students").WithApiVersionSet(apiVersionSet).WithTags("Exercise");

    //        exerciseV1.MapPost("/create", async Task<Results<Ok<string>, NotFound>> (ExerciseService)) =>

    //        return app;
    //    }
    //}
}
