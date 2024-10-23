using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Example;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class ExampleEndpoint
{
    public static WebApplication UseExampleEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();
        
        var examplesV1 = app.MapGroup("v{version:apiVersion}/examples").WithApiVersionSet(apiVersionSet).WithTags("Examples");

        examplesV1.MapPost("/", ([FromBody]ExampleDto dto) =>
        {
            TypedResults.Ok(dto);
        }).WithRequestValidation<ExampleDto>();

        return app;
    }
}