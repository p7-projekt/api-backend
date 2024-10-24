using FluentResults;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class UserService : IUserService
{
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IUserRepository _userRepository;
	private readonly ITokenService _tokenService;
	private readonly ILogger<UserService> _logger;
	public UserService(IPasswordHasher<User> passwordHasher, IUserRepository userRepository, ILogger<UserService> logger, ITokenService tokenService)
	{
		_passwordHasher = passwordHasher;
		_userRepository = userRepository;
		_logger = logger;
		_tokenService = tokenService;
	}

	public async Task CreateUserAsync(CreateUserDto dto)
	{
		var user = new User();
		user.CreatedAt = DateTime.UtcNow;
		user.Email = dto.Email;
		user.Name = dto.Name;
		var passwordHash = _passwordHasher.HashPassword(user, dto.Password);
		user.PasswordHash = passwordHash;

		var createUser = await _userRepository.CreateUserAsync(user, Roles.Instructor);
		_logger.LogInformation("User created: {email} with role: {role}", user.Email, Roles.Instructor);
	}

	public async Task<Result<GetUserResponseDto>> GetAppUserByIdAsync(int id, int userIdParameter)
	{
		if (id != userIdParameter)
		{
			return Result.Fail("User id not consistent");
		}
		var result = await _userRepository.GetAppUserByIdAsync(id);
		_logger.LogInformation("Selecting user with userid: {userid}", id);
		if (result == null)
		{
			return Result.Fail("User not found");
		}

		return Result.Ok(new GetUserResponseDto(result.Email, result.Name));
	}

	public async Task<Result<LoginResponse>> LoginAsync(LoginDto loginDto)
	{
		var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
		if (user == null)
		{
			_logger.LogInformation("User with email {email} not found", loginDto.Email);
			return Result.Fail($"Failed to login user with email {loginDto.Email}");
		}
		var correctPassword = _passwordHasher.VerifyHashedPassword(new User{Email = loginDto.Email, CreatedAt = user.CreatedAt}, user.PasswordHash, loginDto.Password);
		if (correctPassword == PasswordVerificationResult.Failed)
		{
			_logger.LogDebug("Password verification failed for user {email}", loginDto.Email);
			return Result.Fail($"Failed to login user with email {loginDto.Email}");
		}
		var userRoles = await _userRepository.GetRolesByUserIdAsync(user.Id);
		var strUserRoles = string.Concat(userRoles.Select(x => x.RoleName));
		var roles = userRoles.Select(x => RolesConvert.Convert(x.RoleName)).ToList();
		
		_logger.LogInformation("Generating JWT for {id} {email} with role {role}", user.Id, user.Email, strUserRoles);
		var jwtToken = _tokenService.GenerateJwt(user.Id, roles);
		var refreshToken = await _tokenService.GenerateRefreshToken(user.Id);
		return new LoginResponse(jwtToken, refreshToken.Value.Token, refreshToken.Value.Expires);
	}
}