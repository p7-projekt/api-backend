using System.IdentityModel.Tokens.Jwt;
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
	public async Task LoginAsync_UserEqualNull_ShouldReturn_ValidResult()
	{
		// Arrange
		var passwordMock = Substitute.For<IPasswordHasher<User>>();
		var userRepoMock = Substitute.For<IUserRepository>();
		var loggerMock = Substitute.For<ILogger<UserService>>();
		var tokenMock = Substitute.For<ITokenService>();
		var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);
		var loginDto = new LoginDto("Peter@mail.dk", "123456");
		var user = new User
		{
			Email = loginDto.Email,
			CreatedAt = DateTime.UtcNow
		};
		userRepoMock.GetUserByEmailAsync(loginDto.Email).Returns(Task.FromResult(user));
		passwordMock.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(PasswordVerificationResult.Success);
		
		// Act
		var result = await userService.LoginAsync(loginDto);
		
		// Assert
		Assert.True(result.IsSuccess);
		var handler = new JwtSecurityTokenHandler();
		// Assert.True(handler.CanReadToken(result.Value)); This requires a bit more. 
	} 
	// Password verified og ikke verified
	
}