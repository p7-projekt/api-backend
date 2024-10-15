using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Models;
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
			return service.GenerateJwt();
		});
		
		return app;
	}
}