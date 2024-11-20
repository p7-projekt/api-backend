using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Shared;
using FluentResults;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Npgsql.PostgresTypes;
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

	[Fact]
	public async Task CreateUserAsync_ShouldReturn_Ok()
	{
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

		userRepoMock.CreateUserAsync(Arg.Any<User>(), Arg.Any<Roles>()).Returns(Result.Ok());

		var dto = new CreateUserDto("Validmail@mail.com", "Password1!", "Password1!", "James");

		var result = await userService.CreateUserAsync(dto);

		Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateUserAsync_FailOccurInRepository_ShouldReturn_Fail()
    {
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

        userRepoMock.CreateUserAsync(Arg.Any<User>(), Arg.Any<Roles>()).Returns(Result.Fail("Failed to create new user"));

        var dto = new CreateUserDto("Validmail@mail.com", "Password1!", "Password1!", "James");

        var result = await userService.CreateUserAsync(dto);

        Assert.True(result.IsFailed);
    }

	[Fact]
	public async Task GetAppUserByIdAsync_ShouldReturn_Ok()
	{
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

		var user = new User { Email = "validmail@mail.com", Name = "James" };
		userRepoMock.GetUserByIdAsync(Arg.Any<int>()).Returns(user);

		var result = await userService.GetAppUserByIdAsync(1, 1);

		Assert.True(result.IsSuccess);
		Assert.IsType<GetUserResponseDto>(result.Value);
    }

    [Fact]
    public async Task GetAppUserByIdAsync_InconsistentIds_ShouldReturn_Fail()
    {
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

        var user = new User { Email = "validmail@mail.com", Name = "James" };
        userRepoMock.GetUserByIdAsync(Arg.Any<int>()).Returns(user);

		var id1 = 1;
		var id2 = 2;
        var result = await userService.GetAppUserByIdAsync(id1, id2);

        Assert.True(result.IsFailed);
    }

	[Fact]
	public async Task GetAnonUserByIdAsync_ShouldReturn_Ok()
	{
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

		userRepoMock.GetAnonUserSessionByIdAsync(Arg.Any<int>()).Returns(1);
		var user = new User { Anonymous = true };
		userRepoMock.GetUserByIdAsync(Arg.Any<int>()).Returns(user);
		var result = await userService.GetAnonUserByIdAsync(1);

		Assert.True(result.IsSuccess);
		Assert.IsType<GetUserResponseDto>(result.Value);
    }

    [Fact]
    public async Task GetAnonUserByIdAsync_NoUserFound_ShouldReturn_Fail()
    {
        var passwordMock = Substitute.For<IPasswordHasher<User>>();
        var userRepoMock = Substitute.For<IUserRepository>();
        var loggerMock = Substitute.For<ILogger<UserService>>();
        var tokenMock = Substitute.For<ITokenService>();
        var userService = new UserService(passwordMock, userRepoMock, loggerMock, tokenMock);

        userRepoMock.GetAnonUserSessionByIdAsync(Arg.Any<int>()).Returns(0);

        var result = await userService.GetAnonUserByIdAsync(1);

        Assert.True(result.IsFailed);
    }
}