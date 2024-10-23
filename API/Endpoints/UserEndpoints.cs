using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints;

public static class UserEndpoints
{
	public static WebApplication UseUserEndpoints(this WebApplication app)
	{
		ApiVersionSet apiVersionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.ReportApiVersions()
			.Build();

		var usersV1Group = app.MapGroup("v{version:apiVersion}/users").WithApiVersionSet(apiVersionSet)
			.WithTags("Users");

		usersV1Group.MapGet("/{id:int}",
			async Task<Results<Ok<GetUserResponseDto>, NotFound>> (ClaimsPrincipal principal, IUserService service, int id) =>
			{
				var userId = principal.FindFirst(ClaimTypes.UserData)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					return TypedResults.NotFound();
				}

				var user = await service.GetAppUserByIdAsync(Convert.ToInt32(userId), id);
				if (user.IsFailed)
				{
					return TypedResults.NotFound();
				} 

				return TypedResults.Ok(user.Value);
			}).RequireAuthorization(nameof(Roles.Instructor));

		return app;
	}
}