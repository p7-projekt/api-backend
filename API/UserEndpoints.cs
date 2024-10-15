using System.Net;
using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API;

public static class UserEndpoints
{
	public static WebApplication UseUserEndpoints(this WebApplication app)
	{
		ApiVersionSet apiVersionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.ReportApiVersions()
			.Build();

		var usersV1 = app.MapGroup("v{version:apiVersion}/users").WithApiVersionSet(apiVersionSet).WithTags("Users");

		usersV1.MapPost("/", async ([FromBody] CreateUserDto userDto, UserService service) =>
		{
			await service.CreateUser(userDto.Email, userDto.Password);
		}).WithRequestValidation<CreateUserDto>();


		usersV1.MapGet("/token", (TokenService service) =>
		{
			return service.GenerateJwt(1, Roles.Instructor);
		});

		usersV1.MapGet("/anonToken", (TokenService service) =>
		{
			return service.GenerateAnonymousUserJwt(25);
		});

		usersV1.MapGet("/secret", (ClaimsPrincipal user) =>
		{
			foreach (var claim in user.Claims)
			{
				Console.WriteLine(claim.Subject + " " + claim.Value);
			}
			return $"Hello {user.Identity!.Name}, {user.IsInRole(nameof(Roles.Instructor))}";
		}).RequireAuthorization(nameof(Roles.Instructor));

		usersV1.MapPost("/login", async Task<Results<Ok<string>, BadRequest<ValidationProblemDetails>>> ([FromBody] LoginDto loginDto, UserService service) =>
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
		
		return app;
	}
}