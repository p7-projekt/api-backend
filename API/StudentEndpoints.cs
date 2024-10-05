using Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API;

public static class StudentEndpoints
{
    public static WebApplication UseStudentEndpoints(this WebApplication app)
    {
        var students = app.MapGroup("/students").WithTags("Student");


        students.MapGet("/", async Task<Results<Ok<string>, NotFound>> (StudentService s) =>
        {
            await s.HentNogleStudents();
            return TypedResults.Ok("hello");
        });
        
        
        students.MapGet("/hello", () =>
        {
            return "hello";
        });


        return app;
    }
}