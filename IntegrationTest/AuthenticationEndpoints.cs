using System.Net;
using System.Net.Http.Json;
using API;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using IntegrationTest.Setup;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IntegrationTest;

[Collection(CollectionDefinitions.NonParallelCollectionName)]
public class AuthenticationEndpoints : IClassFixture<TestWebApplicationFactory<Program>>
{
	private readonly HttpClient _client;
	private readonly TestWebApplicationFactory<Program> _factory;
	public AuthenticationEndpoints(TestWebApplicationFactory<Program> factory)
	{
		Environment.SetEnvironmentVariable("ConnectionString", "con");
		_factory = factory;
		_client = factory.CreateClient();
	}
	// Refresh token
	[Fact]
	public async Task RefreshToken_ShouldReturn_BadRequest_RefreshTokenNotValid()
	{
		using var scope = _factory.Services.CreateScope();
		var tokenRepo = scope.ServiceProvider.GetService<ITokenRepository>();
		tokenRepo!.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(Result.Fail("failed"));
		var dto = new RefreshDto("token");

		var response = await _client.PostAsJsonAsync("/refresh", dto);
		
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);	
	}
	
	[Fact]
	public async Task RefreshToken_ShouldReturn_BadRequest_UserDoesntExist()
	{
		using var scope = _factory.Services.CreateScope();
		var tokenRepo = scope.ServiceProvider.GetService<ITokenRepository>();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var rf = new RefreshToken{UserId = 1};
		tokenRepo!.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(Result.Ok(rf));
		userRepo!.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(new List<Role>());
		var dto = new RefreshDto("token");

		var response = await _client.PostAsJsonAsync("/refresh", dto);
		
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);	
	}
	[Fact]
	public async Task RefreshToken_ShouldReturn_OkLoginResponse()
	{
		using var scope = _factory.Services.CreateScope();
		var tokenRepo = scope.ServiceProvider.GetService<ITokenRepository>();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var rf = new RefreshToken{UserId = 1};
		tokenRepo!.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(Result.Ok(rf));
		userRepo!.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(new List<Role>{new Role{RoleName = "Instructor"}});
		var dto = new RefreshDto("token");

		var response = await _client.PostAsJsonAsync("/refresh", dto);
		
		
		Assert.True(response.IsSuccessStatusCode);
	}
	
	// login
	[Fact]
	public async Task Login_ShouldReturn_WrongPassword()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var x = new PasswordHasher<User>();
		var user = new User { Id = 1, PasswordHash = "password", Email = "email@email.com", Name = "name", CreatedAt = DateTime.UtcNow};
		var hash = x.HashPassword(user, "pass1");
		user.PasswordHash = hash;
		userRepo!.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(new List<Role>{new Role{RoleName = "Instructor"}});
		userRepo!.GetUserByEmailAsync(Arg.Any<string>()).Returns(user);
		var dto = new LoginDto("email", "pass");
		
		var response = await _client.PostAsJsonAsync("/login", dto);
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
	[Fact]
	public async Task Login_ShouldReturn_OkLoginResponse()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var x = new PasswordHasher<User>();
		var user = new User { Id = 1, PasswordHash = "password", Email = "email@email.com", Name = "name", CreatedAt = DateTime.UtcNow};
		var hash = x.HashPassword(user, "pass");
		user.PasswordHash = hash;
		userRepo!.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(new List<Role>{new Role{RoleName = "Instructor"}});
		userRepo!.GetUserByEmailAsync(Arg.Any<string>()).Returns(user);
		var dto = new LoginDto("email", "pass");
		
		var response = await _client.PostAsJsonAsync("/login", dto);
		
		Assert.True(response.IsSuccessStatusCode);
	}
	[Fact]
	public async Task Login_ShouldReturn_BadRequest_UserDoesntExist()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		userRepo!.GetUserByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult((User?)null));
		var dto = new LoginDto("email", "pass");
		
		var response = await _client.PostAsJsonAsync("/login", dto);
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
	
	// register
	[Fact]
	public async Task Register_ShouldTrigger_RequestValidation()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		var dto = new CreateUserDto("", "Pass!Word212", "Pass!Word212", "Name");
		
		var response = await _client.PostAsJsonAsync("/register", dto);
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Register_ShouldReturn_BadRequest_FailedToCreateUser()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		userRepo!.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);
		userRepo!.CreateUserAsync(Arg.Any<User>(), Arg.Any<Roles>()).Returns(Result.Fail("failed"));
		var dto = new CreateUserDto("email@email.dk", "Pass!Word212", "Pass!Word212", "Name");
		
		var response = await _client.PostAsJsonAsync("/register", dto);
		
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
	
	[Fact]
	public async Task Register_ShouldReturn_Ok()
	{
		using var scope = _factory.Services.CreateScope();
		var userRepo = scope.ServiceProvider.GetService<IUserRepository>();
		userRepo!.IsEmailAvailableAsync(Arg.Any<string>()).Returns(true);
		userRepo!.CreateUserAsync(Arg.Any<User>(), Arg.Any<Roles>()).Returns(Result.Ok());
		var dto = new CreateUserDto("email@email.dk", "Pass!Word212", "Pass!Word212", "Name");
		
		var response = await _client.PostAsJsonAsync("/register", dto);
		
		Assert.True(response.IsSuccessStatusCode);
	}
}