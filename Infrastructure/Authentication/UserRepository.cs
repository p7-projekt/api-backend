using System.Data;
using Dapper;
using FluentResults;
using FluentValidation.Validators;
using Infrastructure.Authentication.Models;
using Infrastructure.Persistence.Contracts;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public class UserRepository
{
	private readonly IDbConnectionFactory _connection;
	private readonly ILogger<UserRepository> _logger;
	
	public UserRepository(IDbConnectionFactory connection, ILogger<UserRepository> logger)
	{
		_connection = connection;
		_logger = logger;
	}

	public async Task<User?> GetUserByIdAsync(int userId)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
		            SELECT * FROM users WHERE id = @id;
		            """;
		var user = await con.QueryAsync<User>(query, new { id = userId });
		return user.FirstOrDefault();
	}

	public async Task<User?> GetUserDetailsByEmailAsync(string email)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
					SELECT users.id, users.email, users.password_hash AS passwordhash, role.name AS rolestr FROM users
						JOIN user_role ON users.id = user_role.user_id
						JOIN role ON user_role.role_id = role.id
					    WHERE email = @email;
					""";
		var user = await con.QuerySingleAsync<User>(query, new { email });
		return user;
	}

	public async Task<bool> IsEmailAvailableAsync(string email)
	{
		using var con = await _connection.CreateConnectionAsync();
		var query = """
					SELECT COUNT(email) FROM users WHERE email = @email;
					""";
		_logger.LogInformation("Checking if email {email} is available", email);
		var result = await con.QueryAsync<int>(query, new { email = email });
		return result.First() == 0;
	}

	public async Task<Result> CreateUserAsync(User user, Roles role)
	{
		using var con = await _connection.CreateConnectionAsync();
		using var transaction = con.BeginTransaction();
		try
		{
			var insertQuery = """
			                 INSERT INTO users (email, password_hash) VALUES (@email, @password_hash) RETURNING id;
			                 """;
			_logger.LogInformation("Inserting user into transaction: {query}", insertQuery);
			var userId = await con.ExecuteScalarAsync<int>(insertQuery, new {email = user.Email, password_hash = user.PasswordHash}, transaction);
			
			var createRoleQuery = """
			                      INSERT INTO user_role (user_id, role_id)
			                      SELECT @userId, id FROM role WHERE name = @roleName;
			                      """;
			_logger.LogInformation("Inserting role in user_roles: {query}", createRoleQuery);
			await con.ExecuteAsync(createRoleQuery, new {userId = userId, roleName = role.ToString()});
			
			_logger.LogInformation("Committing transaction");
			transaction.Commit();
		}
		catch (Exception)
		{
			_logger.LogInformation("Creating user with email: {email} failed", user.Email);
			transaction.Rollback();
			return Result.Fail("Failed to create user");
		}

		return Result.Ok();
	}
	
}
