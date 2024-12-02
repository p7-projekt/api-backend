using System.Net.Http.Json;
using API;
using Core.Languages.Contracts;
using Core.Languages.Models;
using Core.Shared;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class LanguageEndpoints : IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly HttpClient _client;
	private readonly TestWebApplicationFactory<Program> _factory;

	public LanguageEndpoints(TestWebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task GetLanguages_ShouldReturn_LanguageList_200()
	{
		using var scope = _factory.Services.CreateScope();
		var languageSub = scope.ServiceProvider.GetService<ILanguageRepository>();
		var languages = new List<LanguageSupport>()
		{
			new LanguageSupport
			{
				Id = 1,
				Language = "Python",
				Version = "3.0"
			},
			new LanguageSupport
			{
				Id = 2,
				Language = "Haskell",
				Version = "9.8.3"
			},
		};
		languageSub!.GetLanguagesAsync().Returns(languages);
		
		var userId = 1;
		var roles = new List<Roles> { Roles.Instructor };
		_client.AddRoleAuth(userId, roles);
		
		var response = await _client.GetAsync("/v2/languages");
		
		Assert.True(response.IsSuccessStatusCode);
		var obj = await response.Content.ReadFromJsonAsync<List<GetLanguagesResponseDto>>();
		Assert.Equal(2, obj!.Count);
	}
	
	[Fact]
	public async Task GetLanguages_ShouldReturn_EmptyLanguageList_200()
	{
		using var scope = _factory.Services.CreateScope();
		var languageSub = scope.ServiceProvider.GetService<ILanguageRepository>();
		var languages = new List<LanguageSupport>();

		languageSub!.GetLanguagesAsync().Returns(languages);
		
		var userId = 1;
		var roles = new List<Roles> { Roles.Instructor };
		_client.AddRoleAuth(userId, roles);
		
		var response = await _client.GetAsync("/v2/languages");
		
		Assert.True(response.IsSuccessStatusCode);
		var obj = await response.Content.ReadFromJsonAsync<List<GetLanguagesResponseDto>>();
		Assert.Empty(obj!);
	}
}