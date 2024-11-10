using System.Net;
using System.Security.Claims;
using API.Configuration;
using API.Endpoints.Shared;
using Core.Shared;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class AuthenticationEndpoints
{
	public static WebApplication UseAuthenticationEndpoints(this WebApplication app)
	{
		
		var authGroup = app.MapGroup("").WithTags("Authentication");

		authGroup.MapPost("/refresh", async Task<Results<Ok<LoginResponse>, BadRequest<ValidationProblemDetails>>> ([FromBody] RefreshDto refreshDto, ITokenService service) =>
		{
			var result = await service.GenerateJwtFromRefreshToken(refreshDto);
			if (result.IsFailed)
			{
				return TypedResults.BadRequest(CreateBadRequest.CreateValidationProblemDetails(result.Errors, "Bad refresh token", "errors"));
			}
			return TypedResults.Ok(result.Value);
		});
		
		authGroup.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<ValidationProblemDetails>>> ([FromBody] LoginDto loginDto, IUserService service) =>
		{
			var result = await service.LoginAsync(loginDto);
			if (result.IsFailed)
			{
				return TypedResults.BadRequest(
					CreateBadRequest.CreateValidationProblemDetails(result.Errors, "Login failed", "errors"));
			}
			return TypedResults.Ok(result.Value);
		});
		authGroup.MapPost("/register", async Task<Results<Ok, BadRequest<ValidationProblemDetails>>>([FromBody] CreateUserDto userDto, IUserService service) =>
		{
			var result = await service.CreateUserAsync(userDto);
			if (result.IsFailed)
			{
				return TypedResults.BadRequest(
					CreateBadRequest.CreateValidationProblemDetails(result.Errors, "Login failed", "errors"));
			}

			return TypedResults.Ok();
		}).WithRequestValidation<CreateUserDto>();

		return app;
	}
}