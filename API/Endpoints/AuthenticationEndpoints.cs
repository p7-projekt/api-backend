using System.Net;
using System.Security.Claims;
using API.Configuration;
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
				var errors = result.Errors.Select(e => e.Message).ToArray();
				var errorDict = new Dictionary<string, string[]>(); 
				errorDict.Add("error", errors);
				return TypedResults.BadRequest(new ValidationProblemDetails
				{
					Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
					Title = "Authentication error",
					Status = (int)HttpStatusCode.BadRequest,
					Errors = errorDict,
				});
			}
			return TypedResults.Ok(result.Value);
		});
		
		authGroup.MapPost("/login", async Task<Results<Ok<LoginResponse>, BadRequest<ValidationProblemDetails>>> ([FromBody] LoginDto loginDto, IUserService service) =>
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
		// For showcase ####################################################################################
		authGroup.MapGet("/secret",  Ok<string> (ClaimsPrincipal claimsPrincipal) =>
		{
			var user = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.UserData).Value;
			var roles = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.Role).Value;
			return TypedResults.Ok($"Hello user: {user} with role {roles}");
		}).RequireAuthorization(nameof(Roles.Instructor));

		authGroup.MapGet("/anontoken", Ok<string> (ITokenService service) =>
		{
			var token = service.GenerateAnonymousUserJwt(5);
			return TypedResults.Ok(token);
		});
		//###################################################################################################
		authGroup.MapPost("/register", async ([FromBody] CreateUserDto userDto, IUserService service) =>
		{
			await service.CreateUserAsync(userDto.Email, userDto.Password);
		}).WithRequestValidation<CreateUserDto>();

		return app;
	}
}