using System.Net;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints.Shared;

public class CreateBadRequest
{
    public static ValidationProblemDetails CreateValidationProblemDetails(List<IError> errorsInput, string title)
    {
        var errors = errorsInput.Select(e => e.Message).ToArray();
        var errorDict = new Dictionary<string, string[]>(); 
        errorDict.Add("error", errors);
        return new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = title,
            Status = (int)HttpStatusCode.BadRequest,
            Errors = errorDict
        };
    }

    public static ProblemDetails CreateProblemDetails(string title)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = title,
            Status = (int)HttpStatusCode.BadRequest
        };
    }
}