using Asp.Versioning;
using Asp.Versioning.Builder;

namespace API.Endpoints;

public static class SessionEndpoints
{
    public static WebApplication UseSessionEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var sessionV1Group = app.MapGroup("v{version:apiVersion}/sessions").WithApiVersionSet(apiVersionSet)
            .WithTags("Sessions");
        
        // Get session
        sessionV1Group.MapGet("/{id:int}", () =>
        {
            return "get session";
        });
        
        // Create session
        sessionV1Group.MapPost("/", () =>
        {

        });
        
        // Join session
        sessionV1Group.MapPost("/{id:int}/students", () =>
        {

        });
        return app;
    }
}