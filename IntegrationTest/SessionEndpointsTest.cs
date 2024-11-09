using System.Net;
using System.Net.Http.Json;
using API;
using Core.Sessions.Contracts;
using Core.Sessions.Models;
using Core.Shared;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class SessionEndpointsTest: IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly HttpClient _client;
	private readonly TestWebApplicationFactory<Program> _factory;

	public SessionEndpointsTest(TestWebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task GetAuthorSession_ShouldReturn_ListOfSessions()
	{
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
		sessionSub!.GetSessionsAsync(Arg.Any<int>()).Returns(new List<Session>{new Session{Title = "Hello"}});
		var userId = 1;
		var roles = new List<Roles> { Roles.Instructor};
		_client.AddRoleAuth(userId, roles);
		
		var response = await _client.GetAsync("/v1/sessions");
		
		
		Assert.True(response.IsSuccessStatusCode);
		var body = await response.Content.ReadFromJsonAsync<List<GetSessionResponseDto>>();
		Assert.Single(body!);
		Assert.Equal("Hello", body!.First().Title);
		
	}
	
	[Fact]
    public async Task GetAuthorSession_ShouldReturn_FailedResponse()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
    	sessionSub!.GetSessionsAsync(Arg.Any<int>()).Returns(Task.FromResult<IEnumerable<Session>?>(null));
    	var userId = 1;
    	var roles = new List<Roles> { Roles.Instructor};
    	_client.AddRoleAuth(userId, roles);
    	
    	var response = await _client.GetAsync("/v1/sessions");
    	
    	
    	Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
    }
	[Fact]
    public async Task GetAuthorSession_ShouldReturn_FailedAuthentication()
    {
        using var scope = _factory.Services.CreateScope();
        var sessionSub = scope.ServiceProvider.GetService<ISessionRepository>();
    	sessionSub!.GetSessionsAsync(Arg.Any<int>()).Returns(new List<Session>{new Session{Title = "Hello"}}); 
    	var userId = 1;
    	var roles = new List<Roles> { Roles.AnonymousUser};
    	_client.AddRoleAuth(1, roles);
    	
    	var response = await _client.GetAsync("/v1/sessions");
    	
    	
    	Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
    }
}