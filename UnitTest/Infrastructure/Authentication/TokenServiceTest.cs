using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
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
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		var userId = 1;
		var role = new List<Roles>{Roles.Instructor};
		var handler = new JwtSecurityTokenHandler();

		// Act
		var result = service.GenerateJwt(userId, role);
		var token = handler.ReadJwtToken(result);
		
		var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
		var roleClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
		
		// Assert
		Assert.Equal(userId.ToString(), userIdClaim!.Value);
		Assert.Equal(role.First().ToString(), roleClaim!.Value);
	}
	
	[Fact]
	public void GenerateValidJWT_ShouldReturn_ValidJWTTokenWithMultipleRoles()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		var userId = 1;
		var roles = new List<Roles>{Roles.Instructor, Roles.AnonymousUser};
		var handler = new JwtSecurityTokenHandler();

		// Act
		var result = service.GenerateJwt(userId, roles);
		var token = handler.ReadJwtToken(result);
		
		var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
		var roleClaim = token.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
		
		// Assert
		Assert.Equal(userId.ToString(), userIdClaim!.Value);
		for (int i = 0; i < roles.Count; i++)
		{
			Assert.Equal(roles[i].ToString(), roleClaim![i]);
		}
	}

	[Fact]
	public void GenerateValidJwtAnonymousUser_ShouldReturn_ValidJwt()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		var userId = -1;
		var role = Roles.AnonymousUser;
		var sessionLengthInMinutes = 5;
		var handler = new JwtSecurityTokenHandler();
		var currentTime = DateTime.UtcNow;

		// Act
		var result = service.GenerateAnonymousUserJwt(sessionLengthInMinutes);
		var token = handler.ReadJwtToken(result);
		
		var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData);
		var roleClaim = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
		var expectedExpirationTime = currentTime.AddMinutes(sessionLengthInMinutes);
		var expirationTime = token.ValidTo;
		
		// Assert
		Assert.Equal(userId.ToString(), userIdClaim!.Value);
		Assert.Equal(role.ToString(), roleClaim!.Value);
		Assert.True(expirationTime >= expectedExpirationTime.AddSeconds(-1)
		            && expirationTime <= expectedExpirationTime.AddSeconds(1));
	}
}