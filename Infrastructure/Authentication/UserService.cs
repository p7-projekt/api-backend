using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class UserService
{
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly UserRepository _userRepository;
	private readonly ILogger<UserService> _logger;
	public UserService(IPasswordHasher<User> passwordHasher, UserRepository userRepository, ILogger<UserService> logger)
	{
		_passwordHasher = passwordHasher;
		_userRepository = userRepository;
		_logger = logger;
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
	
	
}