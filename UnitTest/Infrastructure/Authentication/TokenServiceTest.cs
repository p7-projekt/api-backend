using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Infrastructure.Authentication;

public class TokenServiceTest
{
	public TokenServiceTest()
	{
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
	}
	[Fact]
	public void GenerateValidJWT_ShouldReturn_ValidJWTToken()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var service = new TokenService(_loggerSubstitute);
		var userId = 1;
		var role = Roles.Instructor;
		var handler = new JwtSecurityTokenHandler();

		// Act
		var result = service.GenerateJwt(userId, role);
		var token = handler.ReadJwtToken(result);
		
		var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
		var roleClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
		
		// Assert
		Assert.Equal(userId.ToString(), userIdClaim!.Value);
		Assert.Equal(role.ToString(), roleClaim!.Value);
	}

	[Fact]
	public void GenerateValidJwtAnonymousUser_ShouldReturn_ValidJwt()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var service = new TokenService(_loggerSubstitute);
		var userId = -1;
		var role = Roles.AnonymousUser;
		var handler = new JwtSecurityTokenHandler();

		// Act
		var result = service.GenerateJwt(userId, role);
		var token = handler.ReadJwtToken(result);
		
		var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
		var roleClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
		
		// Assert
		Assert.Equal(userId.ToString(), userIdClaim!.Value);
		Assert.Equal(role.ToString(), roleClaim!.Value);
	}
}