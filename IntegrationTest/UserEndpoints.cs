using System.Net;
using System.Net.Http.Json;
using API;
using Core.Sessions.Contracts;
using Core.Shared;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Abstractions;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class UserEndpoints : IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly TestWebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;

	public UserEndpoints(TestWebApplicationFactory<Program> factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task GetAnonUser_ShouldReturn_Unauthorized()
	{
		var response = await _client.GetAsync("v1/users/1");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}
	
	[Fact]
	public async Task GetAnonUser_ShouldReturn_NotFoundSessionIdCantBeFound()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		userRepo!.GetAnonUserSessionByIdAsync(Arg.Any<int>()).Returns(0);
		_client.AddAnonAuth(1,1);
		var response = await _client.GetAsync("v1/users/1");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
	
	[Fact]
	public async Task GetAnonUser_ShouldReturn_Ok()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var sessionRepo = scope.ServiceProvider.GetService<ISessionRepository>();
		userRepo!.GetAnonUserSessionByIdAsync(Arg.Any<int>()).Returns(1);
		var user = new User { Anonymous = true };
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
		sessionRepo!.GetTimedSessionIdByUserId(Arg.Any<int>()).Returns(1);
		_client.AddAnonAuth(1,1);
		var response = await _client.GetAsync("v1/users/1");

		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task GetAppUser_ShouldReturn_NotFound()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(Task.FromResult((User?)null));
		_client.AddRoleAuth(1, new List<Roles> {Roles.Instructor});
		var response = await _client.GetAsync("v1/users/1");
		
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
	
	[Fact]
	public async Task GetAppUser_ShouldReturn_Ok()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var user = new User();
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
		_client.AddRoleAuth(1, new List<Roles> {Roles.Instructor});
		var response = await _client.GetAsync("v1/users/1");
		
		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task GetAppUser_ShouldReturn_GetUserResponseDto()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var sessionRepo = scope.ServiceProvider.GetService<ISessionRepository>();
		var user = new User
		{
			Id = 1,
			Email = "test@test.com",
			Name = "test"
		};
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
		sessionRepo!.GetTimedSessionIdByUserId(Arg.Any<int>()).Returns(1);
		_client.AddRoleAuth(1, new List<Roles> {Roles.Student});
		
		var response = await _client.GetAsync("v1/users/1");
		Assert.True(response.IsSuccessStatusCode);
		var obj = await response.Content.ReadFromJsonAsync<GetUserResponseDto>();
		Assert.Equal(user.Email, obj!.Email);
		Assert.Equal(user.Name, obj!.Name);
		Assert.Equal(1, obj!.SessionId);
	}
	
	[Fact]
	public async Task GetAnonUser_ShouldReturn_Error()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var sessionRepo = scope.ServiceProvider.GetService<ISessionRepository>();
		var user = new User
		{
			Id = 1,
			Email = "test@test.com",
			Name = "test",
			Anonymous = false
		};
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
		sessionRepo!.GetTimedSessionIdByUserId(Arg.Any<int>()).Returns(1);
		_client.AddRoleAuth(1, new List<Roles> {Roles.AnonymousUser});
		
		var response = await _client.GetAsync("v1/users/1");
		Assert.True(response.StatusCode == HttpStatusCode.NotFound);
	}
}