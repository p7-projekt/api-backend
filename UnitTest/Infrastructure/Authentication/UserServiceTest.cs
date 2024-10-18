using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTest.Infrastructure.Authentication;

public class UserServiceTest
{
	// User == null og != null
	[Fact]
	public async Task LoginAsync_UserEqualNull_ShouldReturn_FailedResult()
	{
		// Arrange
		var passwordMock = Substitute.For<IPasswordHasher<User>>();
		var userRepoMock = Substitute.For<IUserRepository>();
		var loggerMock = Substitute.For<ILogger<UserService>>();
		var tokenMock = Substitute.For<ITokenService>();
		var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);
		var loginDto = new LoginDto("Peter@mail.dk", "123456");
		userRepoMock.GetUserByEmailAsync(loginDto.Email).Returns(Task.FromResult<User>(null));
		
		// Act
		var result = await userService.LoginAsync(loginDto);
		
		// Assert
		Assert.True(result.IsFailed);
	} 
	
	[Fact]
	public async Task LoginAsync_UserNotNull_ShouldReturn_ValidResult()
	{
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
		// Arrange
		var passwordMock = Substitute.For<IPasswordHasher<User>>();
		var userRepoMock = Substitute.For<IUserRepository>();
		var loggerMock = Substitute.For<ILogger<UserService>>();
		var tokenMock = Substitute.For<ITokenService>();
		var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);
		var loginDto = new LoginDto("Peter@mail.dk", "123456");
		var user = new User
		{
			Id = 1,
			Email = loginDto.Email,
			CreatedAt = DateTime.UtcNow
		};
		userRepoMock.GetUserByEmailAsync(Arg.Any<string>())!.Returns(Task.FromResult(user));
		passwordMock.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(PasswordVerificationResult.Success);
		var roles = new List<Role>
		{
			new Role
			{
				Id = 1,
				RoleName = "Instructor"
			}
		};
		userRepoMock.GetRolesByUserIdAsync(Arg.Any<int>()).Returns(roles);
		var rf = new RefreshToken();
		tokenMock.GenerateRefreshToken(Arg.Any<int>()).Returns(rf);
		// Act
		var result = await userService.LoginAsync(loginDto);
		
		// Assert
		Assert.True(result.IsSuccess);
	} 
	
	[Fact]
	public async Task LoginAsync_UserNotNull_NoValidPassword_ShouldReturn_FailedResult()
	{
		Environment.SetEnvironmentVariable(AuthConstants.JwtSecret, "MySecretJwtKeyForJwtTokens-----------!");
		// Arrange
		var passwordMock = Substitute.For<IPasswordHasher<User>>();
		var userRepoMock = Substitute.For<IUserRepository>();
		var loggerMock = Substitute.For<ILogger<UserService>>();
		var tokenMock = Substitute.For<ITokenService>();
		var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);
		var loginDto = new LoginDto("Peter@mail.dk", "123456");
		var user = new User
		{
			Id = 1,
			Email = loginDto.Email,
			CreatedAt = DateTime.UtcNow
		};
		userRepoMock.GetUserByEmailAsync(Arg.Any<string>())!.Returns(Task.FromResult(user));
		passwordMock.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(PasswordVerificationResult.Failed);
		
		// Act
		var result = await userService.LoginAsync(loginDto);
		
		// Assert
		Assert.True(result.IsFailed);
	} 
	
}