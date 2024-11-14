using System.Security.Claims;
using API.Configuration;
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
				
				// get Role handle different for anon user
				var strRole = principal.FindFirst(ClaimTypes.Role)?.Value;
				var actualRole = RolesConvert.Convert(strRole!);

				if (actualRole == Roles.AnonymousUser)
				{
					var anonDetails = await service.GetAnonUserByIdAsync(Convert.ToInt32(userId));
					if (anonDetails.IsFailed)
					{
						return TypedResults.NotFound();
					}
					return TypedResults.Ok(anonDetails.Value);
				}
				
				var user = await service.GetAppUserByIdAsync(Convert.ToInt32(userId), id);
				if (user.IsFailed)
				{
					return TypedResults.NotFound();
				} 

				return TypedResults.Ok(user.Value);
			}).RequireAuthorization(Policies.AllowAllRoles);

		return app;
	}
}