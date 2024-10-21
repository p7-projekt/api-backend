using Asp.Versioning;
using Asp.Versioning.Builder;
using Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints;

public static class StudentEndpoints
{
    public static WebApplication UseStudentEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();
        
        var studentsV1 = app.MapGroup("v{version:apiVersion}/students").WithApiVersionSet(apiVersionSet).WithTags("Student");


        studentsV1.MapGet("/", async Task<Results<Ok<string>, NotFound>> (StudentService s) =>
        {
            await s.HentNogleStudents();
            return TypedResults.Ok("hello");
        });

        studentsV1.MapPost("/", () =>
        {

        });
        
        studentsV1.MapGet("/hello", () =>
        {
            return "hello";
        });


        return app;
    }
}