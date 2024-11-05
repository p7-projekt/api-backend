using System.Net.Http.Headers;
using Core.Shared;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IntegrationTest.Setup;

public static class AuthHandler
{

	public static void AddAnonAuth(this HttpClient client, int userId, int sessionLength)
	{
		HandleAuth(client, new Auth(true, userId, null, sessionLength));
	}

	public static void AddRoleAuth(this HttpClient client, int userId, List<Roles> roles)
	{
		HandleAuth(client, new Auth(false, userId, roles, null));	
	}

	private static void HandleAuth(HttpClient client, Auth auth)
	{
		var loggerSub = Substitute.For<ILogger<TokenService>>();
        var tokenRepo = Substitute.For<ITokenRepository>();
        var userRepo = Substitute.For<IUserRepository>();
        var tokenService = new TokenService(loggerSub, tokenRepo, userRepo);
        var token = auth.Anon switch
        {
	        true => tokenService.GenerateAnonymousUserJwt(auth.SessionLength!.Value, auth.UserId),
	        false => tokenService.GenerateJwt(auth.UserId, auth.Roles!)
        };
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
	}
	
	private record Auth(bool Anon, int UserId, List<Roles>? Roles, int? SessionLength);
}