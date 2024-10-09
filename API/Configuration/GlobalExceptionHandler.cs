using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Configuration;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var internalServerError = HttpStatusCode.InternalServerError;
        var path = httpContext.Request.Path;
        ProblemDetails errorResponse = new ProblemDetails();
        Console.WriteLine("Hello  324 342342 ");
        switch (exception)
        {
            default:
                errorResponse.Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";
                errorResponse.Title = nameof(HttpStatusCode.InternalServerError);
                errorResponse.Status = (int)internalServerError;
                errorResponse.Detail = exception.Message;
                errorResponse.Instance = path;

            break;
        }

        httpContext.Response.StatusCode = (int)internalServerError;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }
}