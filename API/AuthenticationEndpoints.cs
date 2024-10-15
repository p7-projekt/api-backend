using System.Net;
using API.Configuration;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API;

public static class AuthenticationEndpoints
{
	public static WebApplication UseAuthenticationEndpoints(this WebApplication app)
	{
		
		var authGroup = app.MapGroup("").WithTags("Authentication");

		authGroup.MapPost("/login", async Task<Results<Ok<string>, BadRequest<ValidationProblemDetails>>> ([FromBody] LoginDto loginDto, UserService service) =>
		{
			var result = await service.LoginAsync(loginDto);
			if (result.IsFailed)
			{
				var errors = result.Errors.Select(e => e.Message).ToArray();
				var errorDict = new Dictionary<string, string[]>(); 
				errorDict.Add("error", errors);
				return TypedResults.BadRequest(new ValidationProblemDetails
				{
					Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
					Title = "One or more validation errors occurred.",
					Status = (int)HttpStatusCode.BadRequest,
					Errors = errorDict,
				});
			}
			return TypedResults.Ok(result.Value);
		});
		
		// authGroup.MapPost("/register", async ([FromBody] CreateUserDto userDto, UserService service) =>
		// {
		// 	await service.CreateUser(userDto.Email, userDto.Password);
		// }).WithRequestValidation<CreateUserDto>();

		return app;
	}
}