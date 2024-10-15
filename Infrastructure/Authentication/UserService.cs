using FluentResults;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class UserService
{
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly UserRepository _userRepository;
	private readonly TokenService _tokenService;
	private readonly ILogger<UserService> _logger;
	public UserService(IPasswordHasher<User> passwordHasher, UserRepository userRepository, ILogger<UserService> logger, TokenService tokenService)
	{
		_passwordHasher = passwordHasher;
		_userRepository = userRepository;
		_logger = logger;
		_tokenService = tokenService;
	}

	public async Task CreateUser(string email, string password)
	{
		var user = new User();
		user.Email = email;
		var passwordHash = _passwordHasher.HashPassword(user, password);
		user.PasswordHash = passwordHash;

		var createUser = await _userRepository.CreateUserAsync(user, Roles.Instructor);
		_logger.LogInformation("User created: {email} with role: {role}", user.Email, Roles.Instructor);
	}

	public async Task<Result<string>> LoginAsync(LoginDto loginDto)
	{
		var user = await _userRepository.GetUserDetailsByEmailAsync(loginDto.Email);
		if (user == null)
		{
			_logger.LogInformation("User with email {email} not found", loginDto.Email);
			return Result.Fail($"Failed to login user with email {loginDto.Email}");
		}
		var correctPassword = _passwordHasher.VerifyHashedPassword(new User{Email = loginDto.Email}, user.PasswordHash, loginDto.Password);
		if (correctPassword == PasswordVerificationResult.Failed)
		{
			_logger.LogDebug("Password verification failed for user {email}", loginDto.Email);
			return Result.Fail($"Failed to login user with email {loginDto.Email}");
		}
		_logger.LogInformation("Generating JWT for {id} {email} with role {role}", user.Id, user.Email, user.Role.ToString());
		return _tokenService.GenerateJwt(user.Id, user.Role);
	}
}