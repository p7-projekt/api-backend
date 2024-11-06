using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Exceptions;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Infrastructure.Authentication;

public class TokenServiceTest
{
	public TokenServiceTest()
	{
	}
	[Fact]
	public void GenerateValidJWT_ShouldReturn_ValidJWTToken()
	{
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
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
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
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
	public void MissingJwtSecret_ShouldThrow_MissingJwtKeyException()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, null);
		
		// Act + Assert
		Assert.Throws<MissingJwtKeyException>(() => service.GenerateAnonymousUserJwt(5, -1));
	}

	[Fact]
	public void GenerateValidJwtAnonymousUser_ShouldReturn_ValidJwt()
	{
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
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
		var result = service.GenerateAnonymousUserJwt(sessionLengthInMinutes, userId);
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

	[Fact]
	public async Task GenerateRefreshToken_ShouldReturn_ValidRefreshToken_FromNoValidToken()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		_tokenRepoSubstitute.DeleteRefreshTokenByUserIdAsync(Arg.Any<int>()).Returns(Task.CompletedTask);
		_tokenRepoSubstitute.InsertTokenAsync(Arg.Any<RefreshToken>()).Returns(Task.CompletedTask);
		
		// Act
		var result = await service.GenerateRefreshToken(1);
		
		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
	}
	
	[Fact]
	public async Task GenerateRefreshToken_ShouldReturn_ValidRefreshToken_FromUserId()
	{
		// Arrange
		var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);
		var rf = new RefreshToken
		{
			Id = 0,
			UserId = 1,
			Token = "token",
			CreatedAt = DateTime.UtcNow,
			Expires = DateTime.UtcNow.AddMinutes(1)
		};
		_tokenRepoSubstitute.DeleteRefreshTokenByUserIdAsync(Arg.Any<int>()).Returns(Task.CompletedTask);
		
		// Act
		var result = await service.GenerateRefreshToken(1);
		
		// Assert
		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task GenerateJwtFromRefreshToken_ShouldReturn_Ok()
	{
        Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
        var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
		var _userRepoSubstitute = Substitute.For<IUserRepository>();
		var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
		var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);

		var rf = new RefreshToken
		{
			Id = 0,
			UserId = 1,
			Token = "token",
			CreatedAt = DateTime.UtcNow,
			Expires = DateTime.UtcNow.AddMinutes(1)
		};
		_tokenRepoSubstitute.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(rf);

		Role[] role = { new Role { RoleName = "Instructor" } };
		_userRepoSubstitute.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(role);

		var dto = new RefreshDto("ArbitraryToken");
		var result = await service.GenerateJwtFromRefreshToken(dto);

		Assert.True(result.IsSuccess);
		Assert.IsType<LoginResponse>(result.Value);
    }

    [Fact]
    public async Task GenerateJwtFromRefreshToken_ErrorInTokenRepo_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
        var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
        var _userRepoSubstitute = Substitute.For<IUserRepository>();
        var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
        var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);

        var rf = new RefreshToken
        {
            Id = 0,
            UserId = 1,
            Token = "token",
            CreatedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(1)
        };
        _tokenRepoSubstitute.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(Result.Fail("failed to retrieve token, or token was invalid"));

        Role[] role = { new Role { RoleName = "Instructor" } };
        _userRepoSubstitute.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(role);

        var dto = new RefreshDto("ArbitraryToken");
        var result = await service.GenerateJwtFromRefreshToken(dto);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GenerateJwtFromRefreshToken_NoRolesFound_ShouldReturn_Fail()
    {
        Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
        var _loggerSubstitute = Substitute.For<ILogger<TokenService>>();
        var _userRepoSubstitute = Substitute.For<IUserRepository>();
        var _tokenRepoSubstitute = Substitute.For<ITokenRepository>();
        var service = new TokenService(_loggerSubstitute, _tokenRepoSubstitute, _userRepoSubstitute);

        var rf = new RefreshToken
        {
            Id = 0,
            UserId = 1,
            Token = "token",
            CreatedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(1)
        };
        _tokenRepoSubstitute.GetAccessTokenByRefreshTokenAsync(Arg.Any<string>()).Returns(rf);

        Role[] role = { };
        _userRepoSubstitute.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(role);

        var dto = new RefreshDto("ArbitraryToken");
        var result = await service.GenerateJwtFromRefreshToken(dto);

        Assert.True(result.IsFailed);
    }
}