using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using API;
using Core.Shared;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using IntegrationTest.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
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
	public async Task TestInstructorToken3()
	{


		var userId = 1;
		var role = new List<Roles> { Roles.Instructor };
		_client.AddRoleAuth(userId, role);

		var response = await _client.GetAsync("/secret");

		Assert.True(response.IsSuccessStatusCode);
	}
	
	[Fact]
	public async Task TestAnonToken()
	{
		_client.AddAnonAuth(1, 2);

		var response = await _client.GetAsync("/anontoken");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var responseBody = response.Content.ReadAsStringAsync().Result;
		// Assert.Equal("\"token\"", responseBody);
	}

	[Fact]
    	public async Task TestInstructorToken2()
    	{
    
    
    		var userId = 1;
    		var role = new List<Roles> { Roles.Instructor };
    		_client.AddRoleAuth(userId, role);
		    var p = new ClaimsPrincipal();
		    
    		var response = await _client.GetAsync("/secret");
			var str = response.Content.ReadAsStringAsync().Result; 
			var tokenStr = response.RequestMessage!.Headers.Authorization!.ToString().Split(' ')[1];
			var handler = new JwtSecurityTokenHandler();
			var token = handler.ReadJwtToken(tokenStr);
		
			var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
			var roleClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);	
    		Assert.True(response.IsSuccessStatusCode);
		    Assert.Equal(userId.ToString(), userIdClaim!.Value);
		    Assert.Equal(role.First().ToString(), roleClaim!.Value);
    	}
	
	[Fact]
	public async Task TestAnonToken2()
	{
		_client.AddAnonAuth(1, 2);

		var response = await _client.GetAsync("/anontoken");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var responseBody = response.Content.ReadAsStringAsync().Result;
		// Assert.Equal("\"token\"", responseBody);
	}

	[Fact]
	public async Task TestInstructorToken()
	{


		var userId = 1;
		var role = new List<Roles> { Roles.Instructor };
		_client.AddRoleAuth(userId, role);

		var response = await _client.GetAsync("/secret");

		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task TestAnonToken3()
	{
		_client.AddAnonAuth(1, 2);

		var response = await _client.GetAsync("/anontoken");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var responseBody = response.Content.ReadAsStringAsync().Result;
		// Assert.Equal("\"token\"", responseBody);
	}
}