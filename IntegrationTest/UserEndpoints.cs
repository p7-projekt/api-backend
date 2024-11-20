using System.Net;
using API;
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
		userRepo!.GetAnonUserSessionByIdAsync(Arg.Any<int>()).Returns(1);
		var user = new User { Anonymous = true };
		userRepo!.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
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
}