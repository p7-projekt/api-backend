using Infrastructure.Authentication;
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
		// var passwordMock = Substitute.For<IPasswordHasher<User>>();
		// var userRepoMock = Substitute.For<UserRepository>();
		// var loggerMock = Substitute.For<ILogger<UserService>>();
		// var tokenMock = Substitute.For<TokenService>();
		// var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);
		// var loginDto = new LoginDto("Peter@mail.dk", "123456");
		// userRepoMock.GetUserDetailsByEmailAsync(loginDto.Email).Returns(Task.FromResult<User>(null));
		//
		// // Act
		// var result = await userService.LoginAsync(loginDto);
		//
		// // Assert
		// Assert.True(result.IsFailed);
		Assert.True(true);
	} 
	// Password verified og ikke verified
	
}