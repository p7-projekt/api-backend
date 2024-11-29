using System.Security.Claims;
using API.Configuration;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Core.Languages.Contracts;
using Core.Languages.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints;

public static class LanguageEndpoints
{
	public static WebApplication UseLanguageEndpoints(this WebApplication app, ApiVersionSet apiVersionSet)
	{

		var languageV2 = app.MapGroup("v{version:apiVersion}/languages")
			.WithApiVersionSet(apiVersionSet).MapToApiVersion(2).WithTags("Languages");

		languageV2.MapGet("/", async Task<Results<Ok<List<GetLanguagesResponseDto>>, BadRequest>> (ILanguageService service) =>
		{
			var result = await service.GetLanguages();
			if (result.IsFailed)
			{
				return TypedResults.Ok(Enumerable.Empty<GetLanguagesResponseDto>().ToList());
			}
			return TypedResults.Ok(result.Value);
		}).RequireAuthorization(Policies.AllowAllRoles);

		return app;
	}
}