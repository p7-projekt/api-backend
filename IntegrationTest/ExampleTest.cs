using System.Net;
using API;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IntegrationTest;

public class ExampleTest : IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly TestWebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;

	public ExampleTest(TestWebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task TestAnonToken()
	{
		using var scope = _factory.Services.CreateScope();
		var iTokenSub = scope.ServiceProvider.GetService<ITokenService>();
		iTokenSub.GenerateAnonymousUserJwt(Arg.Any<int>(), Arg.Any<int>()).Returns("token");

		var response = await _client.GetAsync("/anontoken");
		
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var responseBody = response.Content.ReadAsStringAsync().Result;
		Assert.Equal("\"token\"", responseBody);
	}
}